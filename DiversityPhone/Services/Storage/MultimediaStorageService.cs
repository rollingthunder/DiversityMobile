﻿using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.Services;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reactive.Linq;

namespace DiversityPhone
{
    /// <summary>
    /// Interface for specialized handling of Images by the Multimedia Storage Service
    /// </summary>
    public interface IStoreImages : IStoreMultimedia
    {
        /// <summary>
        /// Stores an Image directly from a PhotoResult
        /// </summary>
        /// <param name="fileNameHint">Desired File Name</param>
        /// <param name="image">Task Result containing the Image</param>
        /// <returns>A string URI identifying the image for later retrieval</returns>
        string StoreImage(string fileNameHint, PhotoResult image);

        /// <summary>
        /// Retrieves an Image Thumbnail
        /// </summary>
        /// <param name="uri">A string URI identifying the image</param>        
        Stream GetImageThumbnail(string uri);
    }

    public enum StorageType
    {
        Unknown,
        IsolatedStorage,
        CameraRoll
    }

    public static class MultimediaFileNameMixin
    {
        /// <summary>
        /// Generates a unique Name for a Multimedia File based on the type and owner of the MMO
        /// as well as a TimeStamp
        /// </summary>
        public static string NewFileName(this MultimediaObject This)
        {
            string extension = "jpg";

            switch (This.MediaType)
            {

                case MediaType.Audio:
                    extension = "wav";
                    break;
                case MediaType.Video:
                    extension = "mp4";
                    break;
                case MediaType.Image:
                    break;
                default:
                    break;
            }

            return string.Format("{0}_{1}_{2}.{3}",
                This.OwnerType,
                This.RelatedId,
                DateTime.Now.ToFileTimeStamp(),
                extension);
        }
    }

    public struct StorageDescriptor
    {
        private static readonly Regex ISOSTORE_URI_REGEX = new Regex("^isostore:(?<name>.+)");
        private const string ISOSTORE_URI_FORMAT = "isostore:{0}";
        private static readonly Regex CAMERAROLL_URI_REGEX = new Regex("^cameraroll:/(?<name>.+)");
        private const string CAMERAROLL_URI_FORMAT = "cameraroll:/{0}";

        public StorageType Type { get; set; }
        public string FileName { get; set; }



        public static StorageDescriptor FromURI(string uri)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(uri), "Cannot retrieve StorageDescriptor for empty uri");

            var matches = ISOSTORE_URI_REGEX.Match(uri);
            if (matches.Success)
            {
                return new StorageDescriptor()
                {
                    Type = StorageType.IsolatedStorage,
                    FileName = Path.GetFileName(matches.Groups["name"].Value) //To deal with old URIs that contained a Path, remove in a later version
                };
            }

            matches = CAMERAROLL_URI_REGEX.Match(uri);
            if (matches.Success)
            {
                return new StorageDescriptor() { Type = StorageType.CameraRoll, FileName = matches.Groups["name"].Value };
            }

            return new StorageDescriptor() { Type = StorageType.Unknown, FileName = string.Empty };
        }

        public override string ToString()
        {
            switch (Type)
            {
                case StorageType.IsolatedStorage:
                    return string.Format(ISOSTORE_URI_FORMAT, FileName);
                case StorageType.CameraRoll:
                    return string.Format(CAMERAROLL_URI_FORMAT, FileName);
                case StorageType.Unknown:
                    return string.Empty;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public static class CameraRollMixin
    {
        public static PictureAlbum CameraRoll(this MediaLibrary library)
        {
            return library
                .RootPictureAlbum
                .Albums
                .Where(a => a.Name == "Camera Roll" || a.Name == "Kamerarolle")
                .FirstOrDefault();
        }
    }

    public class MultimediaStorageService : IStoreImages
    {
        public const string MEDIA_FOLDER = "Multimedia";
        private string CurrentMultimediaFolder = null;

        private static Version WP8 = new Version(8, 0);
        private static bool IsWP8 { get { return Environment.OSVersion.Version >= WP8; } }

        private MediaLibrary Library
        {
            get
            {
                return new MediaLibrary();
            }
        }

        public MultimediaStorageService(
            ICurrentProfile Profile
            )
        {
            Profile
                .CurrentProfilePathObservable()
                .Select(p => Path.Combine(p, MEDIA_FOLDER))
                .Do(CreateDirIfNecessary)
                .Subscribe(p => CurrentMultimediaFolder = p);
        }

        public string StoreImage(string fileNameHint, PhotoResult image)
        {
            StorageDescriptor fileDescriptor;
            //On WP8 the image is already saved, check for that...
            if (IsWP8)
            {
                // ... and use the Image in the Camera Roll
                var lib = Library;
                var crImage = lib.CameraRoll().Pictures.OrderByDescending(p => p.Date).First();
                fileDescriptor = new StorageDescriptor() { Type = StorageType.CameraRoll, FileName = crImage.Name };
            }
            else
            {
                var pic = Library.SavePictureToCameraRoll(fileNameHint, image.ChosenPhoto);
                fileDescriptor = new StorageDescriptor() { Type = StorageType.CameraRoll, FileName = pic.Name };
            }
            return fileDescriptor.ToString();
        }

        public Stream GetImageThumbnail(string URI)
        {
            if (URI != null)
            {
                var storageDescriptor = StorageDescriptor.FromURI(URI);

                switch (storageDescriptor.Type)
                {
                    case StorageType.CameraRoll:
                        return GetThumbnailFromCameraRoll(storageDescriptor.FileName);
                    default: // No other Thumbnails supported
                        break;
                }
            }
            return Stream.Null;
        }

        private Stream GetThumbnailFromCameraRoll(string FileName)
        {
            var picture = GetPictureFromCameraRoll(FileName);
            if (picture != null)
            {
                return picture.GetThumbnail();
            }
            return Stream.Null;
        }

        private Picture GetPictureFromCameraRoll(string FileName)
        {
            var cameraRoll = Library.CameraRoll();

            if (cameraRoll != null)
            {
                return cameraRoll.Pictures.Where(p => p.Name == FileName).FirstOrDefault();
            }
            return null;
        }

        private Stream GetMultimediaFromCameraRoll(string FileName)
        {
            var picture = GetPictureFromCameraRoll(FileName);
            if (picture != null)
            {
                return picture.GetImage();
            }
            return Stream.Null;
        }

        private Stream GetMultimediaFromIsolatedStorage(string FileName)
        {
            var result = Stream.Null;

            if (CurrentMultimediaFolder != null)
            {
                var fullPath = Path.Combine(CurrentMultimediaFolder, FileName);

                UsingIsolatedStorage(store =>
                {
                    if (store.FileExists(fullPath))
                    {
                        result = store.OpenFile(fullPath, FileMode.Open, FileAccess.Read);
                    }
                });
            }
            return result;
        }

        private void CreateDirIfNecessary(string dir)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!iso.DirectoryExists(dir))
                {
                    iso.CreateDirectory(dir);
                }
            }
        }

        public string StoreMultimedia(string FileName, Stream data)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(FileName), "Invalid Filename");
            Contract.Requires(CurrentMultimediaFolder != null, "Invalid Profile Folder");

            var filePath = Path.Combine(CurrentMultimediaFolder, FileName);
            var fileDescriptor = new StorageDescriptor() { Type = StorageType.IsolatedStorage, FileName = FileName };
            UsingIsolatedStorage(store =>
                {
                    if (store.FileExists(filePath))
                    {
                        throw new InvalidOperationException("Couldn't save Media. File already Exists!");
                    }
                    else
                    {
                        using (var file = store.CreateFile(filePath))
                        {
                            data.Seek(0, SeekOrigin.Begin);
                            data.CopyTo(file);
                            file.Flush();
                        }
                    }
                });
            return fileDescriptor.ToString();
        }

        public Stream GetMultimedia(string filePath)
        {
            if (filePath != null)
            {
                var storageDescriptor = StorageDescriptor.FromURI(filePath);

                return GetMultimediaFromDescriptor(ref storageDescriptor);
            }
            return Stream.Null;
        }

        public Stream GetMultimediaFromDescriptor(ref StorageDescriptor storageDescriptor)
        {
            switch (storageDescriptor.Type)
            {
                case StorageType.IsolatedStorage:
                    return GetMultimediaFromIsolatedStorage(storageDescriptor.FileName);
                case StorageType.CameraRoll:
                    return GetMultimediaFromCameraRoll(storageDescriptor.FileName);
                case StorageType.Unknown:
                    return Stream.Null;
                default:
                    return Stream.Null;
            }
        }

        public void ClearAllMultimedia()
        {
            UsingIsolatedStorage(store =>
                {
                    if (store.DirectoryExists(MEDIA_FOLDER))
                    {
                        foreach (var file in store.GetFileNames(MEDIA_FOLDER))
                        {
                            store.DeleteFile(string.Format(MEDIA_FOLDER, file));
                        }
                    }
                });
        }

        public void DeleteMultimedia(string uri)
        {
            if (uri != null)
            {
                var storageDescriptor = StorageDescriptor.FromURI(uri);

                if (storageDescriptor.Type == StorageType.IsolatedStorage)
                {
                    UsingIsolatedStorage(store =>
                        {
                            if (store.FileExists(storageDescriptor.FileName))
                                store.DeleteFile(storageDescriptor.FileName);
                        });

                }
            }
        }

        private static void UsingIsolatedStorage(Action<IsolatedStorageFile> action)
        {
            using (var isostore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                action(isostore);
            }
        }
    }
}
