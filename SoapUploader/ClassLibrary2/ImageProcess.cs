using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;


namespace DigiProofs.SoapUpload {
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class ImageProcess {
	BitmapSource bitmap;

	int compression;
	int scale;
	string filename;

	public ImageProcess(string filename, int compression, int scale) {
	    this.filename = filename;
	    this.compression = compression;
	    this.scale = scale;

	    Stream imageStreamSource = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
	    JpegBitmapDecoder decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
	    bitmap = decoder.Frames[0];
	    imageStreamSource.Close();
	}

	// We do scaling and compression here before we create the stream.
	// Note that if the image is scaled, we always compress at at least quality 80.
	public MemoryStream GetImageStream() {
	    if (compression > 0) {
		double max_dimension = bitmap.PixelWidth >= bitmap.PixelHeight ? bitmap.PixelWidth : bitmap.PixelHeight;

		TransformedBitmap scaledBitmapSource = new TransformedBitmap();
		scaledBitmapSource.BeginInit();
		BitmapFrame frameCopy = (BitmapFrame)bitmap.Clone();
		BitmapMetadata metadata = bitmap.Metadata.Clone() as BitmapMetadata;
		//scaledBitmapSource.Source = BitmapFrame.Create(frameCopy, frameCopy.Thumbnail, metadata, frameCopy.ColorContexts);
		scaledBitmapSource.Source = BitmapFrame.Create(bitmap);
		if (max_dimension > scale)
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
		return stream;
	    }
	    else {
		MemoryStream stream = new MemoryStream();
		FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
		byte[] buf = new byte[1048576];
		int count;
		while ((count = fileStream.Read(buf, 0, 1048576)) != 0)
		    stream.Write(buf, 0, count);
		fileStream.Close();
		return stream;
	    }
	}

	public void setCompression(int compression) {
	    this.compression = compression;
	}
    }
}
