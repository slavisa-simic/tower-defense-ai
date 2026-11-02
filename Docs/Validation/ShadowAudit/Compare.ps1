param([string]$BeforeRevision='before', [string]$AfterRevision='after', [string]$OutputPrefix='comparison')
Add-Type -AssemblyName System.Drawing
$auditDirectory = $PSScriptRoot
function Write-Comparison([string]$map, [string[]]$labels, [int[][]]$positions) {
    $canvas = [Drawing.Bitmap]::new(1140, 716)
    $graphics = [Drawing.Graphics]::FromImage($canvas)
    $graphics.Clear([Drawing.Color]::FromArgb(25, 29, 31))
    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
    $font = [Drawing.Font]::new('Arial', 18, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
    $brush = [Drawing.Brushes]::White
    for ($row=0; $row -lt 2; $row++) {
        $revision = @($BeforeRevision,$AfterRevision)[$row]
        $source = [Drawing.Image]::FromFile((Join-Path $auditDirectory "$revision-$map.png"))
        for ($col=0; $col -lt 3; $col++) {
            $label = @('before','after')[$row]
            $graphics.DrawString("$label - $($labels[$col])", $font, $brush, [single]($col*380+10), [single]($row*358+8))
            $destination = [Drawing.Rectangle]::new($col*380+10,$row*358+42,360,300)
            $crop = [Drawing.Rectangle]::new($positions[$col][0],$positions[$col][1],180,150)
            $graphics.DrawImage($source,$destination,$crop,[Drawing.GraphicsUnit]::Pixel)
        }
        $source.Dispose()
    }
    $canvas.Save((Join-Path $auditDirectory "$OutputPrefix-$map.png"),[Drawing.Imaging.ImageFormat]::Png)
    $font.Dispose(); $graphics.Dispose(); $canvas.Dispose()
}
# Identical screenshot crops, 2x nearest-neighbor, labels only. No runtime art changes.
Write-Comparison 'DanubeIndustrialPort' @('containers','crane','warehouse / booth') @(@(780,220),@(635,164),@(1200,184))
Write-Comparison 'CoastalDefense_TopDown' @('command post','defense footprints','creature contacts') @(@(1290,435),@(1100,266),@(610,285))
