using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;


namespace UploadExpress {
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class ImageProcess {
	BitmapSource bitmap;

	int compression;
	int scale;
        private Stream imageStream;

	public ImageProcess(Stream imageStream, int compression, int scale) {
	    this.imageStream = imageStream;
	    this.compression = compression;
	    this.scale = scale;

	    JpegBitmapDecoder decoder = new JpegBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
	    bitmap = decoder.Frames[0];
	    imageStream.Close();
	}

	// We do scaling and compression here before we create the stream.
	// Note that if the image is scaled, we always compress at at least quality 80.
	public Stream GetImageStream() {
	    double max_dimension = bitmap.PixelWidth >= bitmap.PixelHeight ? bitmap.PixelWidth : bitmap.PixelHeight;

	    TransformedBitmap scaledBitmapSource = new TransformedBitmap();
	    scaledBitmapSource.BeginInit();
	    BitmapFrame frameCopy = (BitmapFrame)bitmap.Clone();
	    BitmapMetadata metadata = bitmap.Metadata.Clone() as BitmapMetadata;
	    scaledBitmapSource.Source = BitmapFrame.Create(bitmap);
	    if (scale > 0 && max_dimension > scale)
		scaledBitmapSource.Transform = new ScaleTransform(scale / max_dimension, scale / max_dimension);
	    scaledBitmapSource.EndInit();

	    MemoryStream stream = new MemoryStream();
	    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
	    encoder.QualityLevel = compression;
	    uint padding = 2048;
	    metadata.SetQuery("/app1/ifd/PaddingSchema:Padding", padding);
	    metadata.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", padding);
	    metadata.SetQuery("/xmp/PaddingSchema:Padding", padding);
	    encoder.Frames.Add(BitmapFrame.Create(scaledBitmapSource, null, metadata, frameCopy.ColorContexts));
	    encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin); // seek 0 or the stream will not properly read. 
            return stream;
	}

    }
}
