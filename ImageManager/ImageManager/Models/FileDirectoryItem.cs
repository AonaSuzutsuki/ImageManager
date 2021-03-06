﻿using ImageManager.ImageLoader;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageManager.Models
{
    public class FileDirectoryItem : BindableBase
    {
        private static BitmapSource bitmapSource = null;
        
        private BitmapSource imageSource;

        public int Id { get; set; } = 0;
        public bool IsDirectory { get; set; } = false;
        public BitmapSource ImageSource
        {
            get => imageSource;
            set => SetProperty(ref imageSource, value);
        }
        public string Text { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string Mimetype { get; set; } = string.Empty;
        
        public FileDirectoryItem()
        {
            if (bitmapSource == null)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("ImageManager.Images.no-image.png"))
                {
                    bitmapSource = ImageConverter.GetBitmapImage(stream);
                    bitmapSource.Freeze();
                }
            }

            ImageSource = bitmapSource;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[ Id = {0}, IsDirectory = {1}, ImageSource = {2}, Text = {3} ]", Id, IsDirectory, ImageSource, Text);
            return sb.ToString();
        }
    }
}
