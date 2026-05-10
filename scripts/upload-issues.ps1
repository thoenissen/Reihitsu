[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $false)]
    [string]$DraftDirectory = "plans\issues",

    [Parameter(Mandatory = $false)]
    [string]$Repository = "",

    [Parameter(Mandatory = $false)]
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-FrontMatter {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ($Lines.Count -lt 3 -or $Lines[0].Trim() -ne "---") {
        throw "Missing YAML front matter in '$Path'."
    }

    $endIndex = -1
    for ($index = 1; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index].Trim() -eq "---") {
            $endIndex = $index
            break
        }
    }

    if ($endIndex -lt 0) {
        throw "Unclosed YAML front matter in '$Path'."
    }

    $metadata = @{}
    for ($index = 1; $index -lt $endIndex; $index++) {
        $line = $Lines[$index].Trim()
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -notmatch "^(?<key>[^:]+):\s*(?<value>.*)$") {
            throw "Invalid front matter line '$line' in '$Path'."
        }

        $key = $Matches["key"].Trim().ToLowerInvariant()
        $value = $Matches["value"].Trim()
        $value = $value.Trim("'`"")
        $metadata[$key] = $value
    }

    if (-not $metadata.ContainsKey("template")) {
        throw "Front matter in '$Path' is missing 'template'."
    }

    if (-not $metadata.ContainsKey("title")) {
        throw "Front matter in '$Path' is missing 'title'."
    }

    $bodyLines = @()
    if ($endIndex + 1 -lt $Lines.Count) {
        $bodyLines = $Lines[($endIndex + 1)..($Lines.Count - 1)]
    }

    return @{
        Metadata = $metadata
        Body = ($bodyLines -join [Environment]::NewLine).Trim()
    }
}

function Test-TemplateBody {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Template,

        [Parameter(Mandatory = $true)]
        [string]$Body,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $requiredHeadingsByTemplate = @{
        "analyzer_feature_request" = @(
            "Motivation",
            "Proposed Solution",
            "Should this feature include a code fix?",
            "Alternatives Considered",
            "Example That Should Trigger the Rule",
            "Preferred Compliant Example",
            "Additional Context"
        )
        "analyzer_bug_report" = @(
            "Description",
            "Expected Behavior",
            "Actual Behavior",
            "Minimal Reproducible Example",
            "Diagnostic ID",
            "Does the issue involve a code fix?",
            "Analyzer Version",
            ".NET Version",
            "IDE",
            "Operating System",
            "Logs or Additional Context"
        )
        "formatter_feature_request" = @(
            "Motivation",
            "Proposed Behavior",
            "Affected Surface",
            "Alternatives Considered",
            "Input Example",
            "Desired Formatted Output",
            "Additional Context"
        )
        "formatter_bug_report" = @(
            "Description",
            "Expected Behavior",
            "Actual Behavior",
            "Input Code",
            "Expected Formatted Output",
            "Actual Formatted Output",
            "CLI Version",
            "Operating System",
            "Logs or Additional Context"
        )
        "code_review_report" = @(
            "Context",
            "Report Goal",
            "Questions To Answer",
            "Acceptance Criteria",
            "References",
            "Additional Context"
        )
    }

    $templateKey = $Template.Trim().ToLowerInvariant()
    if (-not $requiredHeadingsByTemplate.ContainsKey($templateKey)) {
        throw "Unknown template '$Template' in '$Path'."
    }

    foreach ($heading in $requiredHeadingsByTemplate[$templateKey]) {
        $escapedHeading = [Regex]::Escape($heading)
        if ($Body -notmatch "(?m)^###\s+$escapedHeading\s*$") {
            throw "Template validation failed for '$Path': missing heading '### $heading'."
        }
    }
}

function Get-LabelList {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Metadata
    )

    if (-not $Metadata.ContainsKey("labels")) {
        return @()
    }

    return $Metadata["labels"].Split(",", [StringSplitOptions]::RemoveEmptyEntries) |
        ForEach-Object { $_.Trim() } |
        Where-Object { [string]::IsNullOrWhiteSpace($_) -eq $false }
}

function Invoke-GitHubCli {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$ErrorContext
    )

    $result = & gh @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $errorText = ($result | Out-String).Trim()
        if ([string]::IsNullOrWhiteSpace($errorText)) {
            $errorText = "gh exited with code $exitCode."
        }

        throw "$ErrorContext $errorText"
    }

    return @($result)
}

function Resolve-RepositoryName {
    param(
        [Parameter(Mandatory = $false)]
        [string]$RequestedRepository
    )

    if ([string]::IsNullOrWhiteSpace($RequestedRepository) -eq $false) {
        return $RequestedRepository.Trim()
    }

    $repoOutput = Invoke-GitHubCli -Arguments @("repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner") -ErrorContext "Failed to resolve current repository."
    $resolvedRepository = ($repoOutput | Select-Object -Last 1).ToString().Trim()
    if ([string]::IsNullOrWhiteSpace($resolvedRepository)) {
        throw "Failed to resolve current repository. Provide -Repository explicitly."
    }

    return $resolvedRepository
}

function Get-ExistingRepositoryLabels {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedRepository
    )

    $labelOutput = Invoke-GitHubCli -Arguments @("api", "repos/$ResolvedRepository/labels", "--paginate", "--jq", ".[].name") -ErrorContext "Failed to fetch repository labels."
    $labelSet = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
    foreach ($label in $labelOutput) {
        $labelName = $label.ToString().Trim()
        if ([string]::IsNullOrWhiteSpace($labelName) -eq $false) {
            [void]$labelSet.Add($labelName)
        }
    }

    return $labelSet
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) was not found in PATH."
}

$resolvedDraftDirectory = Resolve-Path -Path $DraftDirectory
$resolvedRepository = Resolve-RepositoryName -RequestedRepository $Repository
$existingLabelSet = Get-ExistingRepositoryLabels -ResolvedRepository $resolvedRepository
$draftFiles = @(Get-ChildItem -Path $resolvedDraftDirectory -Filter "*.md" -File | Sort-Object Name)

if ($draftFiles.Count -eq 0) {
    throw "No issue draft files (*.md) found in '$resolvedDraftDirectory'."
}

$issuesToCreate = @()
foreach ($file in $draftFiles) {
    $lines = @(Get-Content -Path $file.FullName)
    if ($lines.Count -eq 0 -or ($lines.Count -eq 1 -and [string]::IsNullOrWhiteSpace($lines[0]))) {
        throw "Issue draft file is empty in '$($file.FullName)'."
    }

    $parsed = Get-FrontMatter -Lines $lines -Path $file.FullName
    $metadata = $parsed.Metadata
    $body = $parsed.Body

    if ([string]::IsNullOrWhiteSpace($body)) {
        throw "Issue body is empty in '$($file.FullName)'."
    }

    Test-TemplateBody -Template $metadata["template"] -Body $body -Path $file.FullName

    $issuesToCreate += [PSCustomObject]@{
        Path = $file.FullName
        FileName = $file.Name
        Title = $metadata["title"]
        Template = $metadata["template"]
        Labels = Get-LabelList -Metadata $metadata
        Body = $body
    }
}

Write-Host "Target repository: $resolvedRepository" -ForegroundColor Cyan
Write-Host "Prepared $($issuesToCreate.Count) issue draft(s):" -ForegroundColor Cyan
foreach ($issue in $issuesToCreate) {
    $labelText = if ($issue.Labels.Count -gt 0) { $issue.Labels -join ", " } else { "<none>" }
    Write-Host " - $($issue.FileName): $($issue.Title) [template=$($issue.Template), labels=$labelText]"
}

if (-not $Force) {
    $confirmation = Read-Host "Type UPLOAD to create these issues"
    if ($confirmation -ne "UPLOAD") {
        Write-Host "Aborted. No issues were created." -ForegroundColor Yellow
        exit 0
    }
}

$createdIssueUrls = @()
foreach ($issue in $issuesToCreate) {
    $tempBodyFile = New-TemporaryFile
    try {
        Set-Content -Path $tempBodyFile -Value $issue.Body -NoNewline

        $validLabels = @()
        $missingLabels = @()
        foreach ($label in $issue.Labels) {
            if ($existingLabelSet.Contains($label)) {
                $validLabels += $label
            }
            else {
                $missingLabels += $label
            }
        }

        if ($missingLabels.Count -gt 0) {
            Write-Warning "Skipping missing labels for '$($issue.FileName)': $($missingLabels -join ', ')"
        }

        $ghArgs = @("issue", "create", "--repo", $resolvedRepository, "--title", $issue.Title, "--body-file", $tempBodyFile)
        foreach ($label in $validLabels) {
            $ghArgs += @("--label", $label)
        }

        if ($PSCmdlet.ShouldProcess($issue.Title, "Create GitHub issue")) {
            $result = Invoke-GitHubCli -Arguments $ghArgs -ErrorContext "Failed to create issue from '$($issue.FileName)'."
            $urlLine = $result | Where-Object { $_ -match "^https://github\.com/.+/issues/\d+$" } | Select-Object -Last 1
            $url = if ($null -ne $urlLine) { $urlLine.ToString().Trim() } else { "" }
            if ([string]::IsNullOrWhiteSpace($url)) {
                throw "Issue created from '$($issue.FileName)' but no issue URL was returned. Raw output: $((($result | Out-String).Trim()))"
            }

            $createdIssueUrls += $url
            Write-Host "Created: $url" -ForegroundColor Green
        }
    }
    finally {
        Remove-Item -Path $tempBodyFile -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "Done. Created $($createdIssueUrls.Count) issue(s)." -ForegroundColor Cyan
