# Certificate template

Drop the certificate background PNG at this exact path:

`backend/src/ZealEducation.Infrastructure/Pdf/Templates/certificate-template.png`

Recommended size: A4 landscape proportions (e.g. 1500x1100 px or larger). The
PDF generator stretches the image to fill A4 landscape via `FitArea()`, so use
an image whose aspect ratio is close to A4 landscape (≈ 1.414 : 1) to avoid
distortion / letterboxing.

The file is loaded once at startup by `QuestPdfCertificateGenerator` and cached
in memory. If it is missing, the generator throws `InvalidOperationException`
with a clear message.

This README has no runtime effect — the matching `certificate-template.png` is
the only file required.
