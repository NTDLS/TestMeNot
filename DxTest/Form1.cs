using SharpCompress.Archives;
using SharpCompress.Common;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace DxTest
{
    public partial class Form1 : Form
    {
        private RenderTarget renderTarget;
        private readonly ImagingFactory _wicFactory = new ImagingFactory();

        public Form1()
        {
            InitializeComponent();

            // Initialize the Direct2D factory and rendering context
            var d2dFactory = new Factory();

            var renderProp = new HwndRenderTargetProperties()
            {
                PresentOptions = PresentOptions.Immediately,
                Hwnd = this.Handle,
                PixelSize = new Size2(this.Width, this.Height),
            };

            renderTarget = new WindowRenderTarget(d2dFactory, new RenderTargetProperties(), renderProp);

            // Handle the Paint event to draw the bitmap
            Paint += MainForm_Paint;

            CacheAllAssets();
        }

        private void CacheAllAssets()
        {
            using (var archive = ArchiveFactory.Open("C:\\Program Files\\NetworkDLS\\Strikeforce Infinity\\Si.Assets.rez"))
            {
                foreach (var entry in archive.Entries)
                {
                    switch (Path.GetExtension(entry.Key).ToLower())
                    {
                        case ".png":
                            LoadBitmapFromFileStream(entry);
                            break;
                    }
                }
            }
        }


 

        private MemoryStream GetCompressedStream(IArchiveEntry entry)
        {
                    using var stream = entry.OpenEntryStream();
                    var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;

        }

        private SharpDX.Direct2D1.Bitmap LoadBitmapFromFileStream(IArchiveEntry entry)
        {
            var stream = GetCompressedStream(entry);
            using var decoder = new BitmapDecoder(_wicFactory, stream, DecodeOptions.CacheOnLoad);
            using var frame = decoder.GetFrame(0);
            using var converter = new FormatConverter(_wicFactory);
            converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPBGRA);

            return SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, converter);
        }


        private SharpDX.Direct2D1.Bitmap LoadBitmapFromFilePath(string filePath)
        {
            using var decoder = new BitmapDecoder(_wicFactory, filePath, DecodeOptions.CacheOnLoad);
            using var frame = decoder.GetFrame(0);
            using var converter = new FormatConverter(_wicFactory);
            converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPBGRA);

            return SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, converter);
        }

        private void MainForm_Paint(object? sender, PaintEventArgs e)
        {
            RawColor4 clearColor = new RawColor4(0.0f, 0.0f, 0.0f, 1.0f); // White color

            renderTarget.BeginDraw();
            renderTarget.Clear(clearColor);

            var bitmap = LoadBitmapFromFilePath("C:\\NTDLS\\TestMeNot\\32.png");

            DrawBitmapAt(renderTarget, bitmap, 100, 100);

            renderTarget.EndDraw();
        }

        public RawRectangleF DrawBitmapAt(RenderTarget renderTarget, SharpDX.Direct2D1.Bitmap bitmap, double x, double y/*, double angle*/)
        {
            var destRect = new RawRectangleF((float)x, (float)y, (float)(x + bitmap.PixelSize.Width), (float)(y + bitmap.PixelSize.Height));
            //SetTransformAngle(renderTarget, destRect, angle);
            renderTarget.DrawBitmap(bitmap, destRect, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            //ResetTransform(renderTarget);
            return destRect;
        }

    }
}
