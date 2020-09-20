using Common.WinApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DataFormats = System.Windows.Forms.DataFormats;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace Common
{
    public static class Utils
    {
        public static void LightenImage(Bitmap bitmap)
        {
            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var alpha = 128;

            using var graphics = Graphics.FromImage(bitmap);
            using var cloud_brush = new SolidBrush(System.Drawing.Color.FromArgb(alpha, System.Drawing.Color.White));
            graphics.FillRectangle(cloud_brush, rectangle);

        }

        public static MemoryStream BitmapSourceToStream(BitmapSource bitmapSource)
        {
            var memoryStream = new MemoryStream();
            //new PngBitmapEncoder()
            //new JpegBitmapEncoder();
            var bitmapEncoder = new JpegBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            bitmapEncoder.Save(memoryStream);

            return memoryStream;
        }

        public static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            var bitmap = new Bitmap(
                  bitmapSource.PixelWidth,
                  bitmapSource.PixelHeight,
                  System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            var bitmapData = bitmap.LockBits(
                  new Rectangle(System.Drawing.Point.Empty, bitmap.Size),
                  ImageLockMode.WriteOnly,
                  System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            bitmapSource.CopyPixels(
                  Int32Rect.Empty,
                  bitmapData.Scan0,
                  bitmapData.Height * bitmapData.Stride,
                  bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            var writeableBitmap = new WriteableBitmap(1280, 1024, 96.0, 96.0, PixelFormats.Bgr24, null);

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            writeableBitmap.Lock();
            
            Interop.CopyMemory(
                writeableBitmap.BackBuffer,
                data.Scan0,
                (uint)(writeableBitmap.BackBufferStride * bitmap.Height));

            //var bitmapData = bitmap.LockBits(
            //    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            //    ImageLockMode.ReadOnly,
            //    bitmap.PixelFormat);

            //var bitmapSource = BitmapSource.Create(
            //    bitmapData.Width,
            //    bitmapData.Height,
            //    bitmap.HorizontalResolution,
            //    bitmap.VerticalResolution,
            //    PixelFormats.Bgr24,
            //    null,
            //    bitmapData.Scan0,
            //    bitmapData.Stride * bitmapData.Height,
            //    bitmapData.Stride);

            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.Width, bitmap.Height));
            writeableBitmap.Unlock();

            bitmap.UnlockBits(data);

            return writeableBitmap;
        }

        public static BitmapImage ImageToBitmapImage(Image image)
        {
            using var memoryStream = new MemoryStream();

            image.Save(memoryStream, ImageFormat.Bmp);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            
            try
            {
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());

                
                var encoder = new JpegBitmapEncoder();
                var memoryStream = new MemoryStream();
                var bitmapImage = new BitmapImage();

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);

                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                memoryStream.Close();

                return bitmapImage;
            }
            catch(Exception ex)
            {
                return null;
            }
            finally
            {
                Interop.DeleteObject(hBitmap);
            }
        }

        public static BitmapSource Convert(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static MemoryStream GetStreamByFileContents(this IDataObject dataObject)
        {
            var formsDataObject = dataObject as System.Windows.DataObject;

            var fileContentNames = (string[])formsDataObject.GetData("FileGroupDescriptor");

            var fileContents = new MemoryStream[fileContentNames.Length];

            for (int fileIndex = 0; fileIndex < fileContentNames.Length; fileIndex++)
            {
                fileContents[fileIndex] = GetData(dataObject, "FileContents", fileIndex);
            }
            
            return fileContents[0];
        }

        public static BitmapSource GetBitmapSourceByDragImageBits(this IDataObject dataObject)
        {
            var windowsDataObject = dataObject as System.Windows.DataObject;

            var imageStream = (MemoryStream)windowsDataObject.GetData("DragImageBits");
            imageStream.Seek(0, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(imageStream);

            ShDragImage shDragImage;
            shDragImage.sizeDragImage.cx = binaryReader.ReadInt32();
            shDragImage.sizeDragImage.cy = binaryReader.ReadInt32();
            shDragImage.ptOffset.x = binaryReader.ReadInt32();
            shDragImage.ptOffset.y = binaryReader.ReadInt32();
            shDragImage.hbmpDragImage = new IntPtr(binaryReader.ReadInt32());
            shDragImage.crColorKey = binaryReader.ReadInt32();
            int stride = shDragImage.sizeDragImage.cx * 4;
            var imageData = new byte[stride * shDragImage.sizeDragImage.cy];

            for (int index = (shDragImage.sizeDragImage.cy - 1) * stride; index >= 0; index -= stride)
            {
                binaryReader.Read(imageData, index, stride);
            }

            var bitmapSource = BitmapSource.Create(
                shDragImage.sizeDragImage.cx,
                shDragImage.sizeDragImage.cy,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                imageData,
                stride);

            return bitmapSource;
        }

        public static MemoryStream GetStreamByFileGroupDescriptorW(this IDataObject dataObject)
        {
            var fileGroupDescriptorWPointer = IntPtr.Zero;

            var formsDataObject = dataObject as System.Windows.DataObject;

            try
            {
                var fileGroupDescriptorStream = (MemoryStream)formsDataObject.GetData("FileGroupDescriptorW");
                var fileGroupDescriptorBytes = new byte[fileGroupDescriptorStream.Length];
                fileGroupDescriptorStream.Read(fileGroupDescriptorBytes, 0, fileGroupDescriptorBytes.Length);
                fileGroupDescriptorStream.Close();

                fileGroupDescriptorWPointer = Marshal.AllocHGlobal(fileGroupDescriptorBytes.Length);
                Marshal.Copy(fileGroupDescriptorBytes, 0, fileGroupDescriptorWPointer, fileGroupDescriptorBytes.Length);

                var fileGroupDescriptor = Marshal.PtrToStructure<FileGroupDescriptorW>(fileGroupDescriptorWPointer);

                var fileNames = new string[fileGroupDescriptor.cItems];

                var fileDescriptorPointer = (IntPtr)((long)fileGroupDescriptorWPointer + Marshal.SizeOf(fileGroupDescriptor.cItems));

                for (int fileDescriptorIndex = 0; fileDescriptorIndex < fileGroupDescriptor.cItems; fileDescriptorIndex++)
                {
                    var fileDescriptor = Marshal.PtrToStructure<FileDescriptorW>(fileDescriptorPointer);
                    fileNames[fileDescriptorIndex] = fileDescriptor.cFileName;

                    fileDescriptorPointer = (IntPtr)((long)fileDescriptorPointer + Marshal.SizeOf(fileDescriptor));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(fileGroupDescriptorWPointer);
            }

            return GetData(dataObject, "FileContents", 0);
        }

        /// <summary>
        /// Retrieves the data associated with the specified data format at the specified index.
        /// </summary>
        /// <param name="format">The format of the data to retrieve. See <see cref="T:System.Windows.Forms.DataFormats"></see> for predefined formats.</param>
        /// <param name="index">The index of the data to retrieve.</param>
        /// <returns>
        /// A <see cref="MemoryStream"/> containing the raw data for the specified data format at the specified index.
        /// </returns>
        public static MemoryStream GetData(this IDataObject dataObject, string format, int index)
        {
            var formatetc = new FORMATETC();
            formatetc.cfFormat = (short)DataFormats.GetFormat(format).Id;
            formatetc.dwAspect = DVASPECT.DVASPECT_CONTENT;
            formatetc.lindex = index;
            formatetc.ptd = new IntPtr(0);
            formatetc.tymed = TYMED.TYMED_ISTREAM | TYMED.TYMED_ISTORAGE | TYMED.TYMED_HGLOBAL;

            dataObject.GetData(ref formatetc, out STGMEDIUM medium);

            MemoryStream stream = null;

            switch (medium.tymed)
            {
                case TYMED.TYMED_ISTORAGE:
                    //to handle a IStorage it needs to be written into a second unmanaged
                    //memory mapped storage and then the data can be read from memory into
                    //a managed byte and returned as a MemoryStream

                    IStorage sourceStorage = null;
                    IStorage destinationStorage = null;
                    ILockBytes lockBytes = null;

                    try
                    {
                        sourceStorage = (IStorage)Marshal.GetObjectForIUnknown(medium.unionmember);
                        Marshal.Release(medium.unionmember);

                        //create a ILockBytes (unmanaged byte array) and then create a IStorage using the byte array as a backing store
                        lockBytes = Interop.CreateILockBytesOnHGlobal(IntPtr.Zero, true);
                        destinationStorage = Interop.StgCreateDocfileOnILockBytes(lockBytes, 0x00001012, 0);

                        //copy the returned IStorage into the new IStorage
                        sourceStorage.CopyTo(0, null, IntPtr.Zero, destinationStorage);
                        lockBytes.Flush();
                        destinationStorage.Commit(0);

                        //get the STATSTG of the ILockBytes to determine how many bytes were written to it
                        var lockBytesStat = new STATSTG();
                        lockBytes.Stat(out lockBytesStat, 1);
                        var lockBytesSize = (int)lockBytesStat.cbSize;

                        //read the data from the ILockBytes (unmanaged byte array) into a managed byte array
                        var bytes = new byte[lockBytesSize];
                        lockBytes.ReadAt(0, bytes, bytes.Length, null);
                        
                        stream = new MemoryStream(bytes);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(destinationStorage);
                        Marshal.ReleaseComObject(lockBytes);
                        Marshal.ReleaseComObject(sourceStorage);
                    }

                    return stream;

                case TYMED.TYMED_ISTREAM:
                    //to handle a IStream it needs to be read into a managed byte and
                    //returned as a MemoryStream

                    IStream iStream = null;

                    try
                    {
                        //marshal the returned pointer to a IStream object
                        iStream = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
                        Marshal.Release(medium.unionmember);

                        //get the STATSTG of the IStream to determine how many bytes are in it
                        var iStreamStat = new STATSTG();
                        iStream.Stat(out iStreamStat, 0);
                        int iStreamSize = (int)iStreamStat.cbSize;

                        var bytes = new byte[iStreamSize];
                        iStream.Read(bytes, bytes.Length, IntPtr.Zero);
                        stream = new MemoryStream(bytes);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(iStream);
                    }

                    return stream;
                case TYMED.TYMED_HGLOBAL:

                    byte[] fileContent = null;
                    var mediumLock = Interop.GlobalLock(medium.unionmember);

                    try
                    {
                        var size = Interop.GlobalSize(mediumLock);
                        fileContent = new byte[size];
                        Marshal.Copy(mediumLock, fileContent, 0, size);
                        stream = new MemoryStream(fileContent, false);
                    }
                    finally
                    {
                        Interop.GlobalUnlock(medium.unionmember);
                    }

                    return stream;
            }

            return null;
        }
    }
}
