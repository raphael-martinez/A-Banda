using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Playmove
{
    [StructLayout(LayoutKind.Sequential)]
    public class PYStorage : MonoBehaviour
    {
        private static PYStorage _instance;

        public static PYStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = (new GameObject("PYStorage")).AddComponent<PYStorage>();
                return _instance;
            }
        }

        public const int ANY_TYPE_DEVICE = -1;

        #region Data for Async operations

        public class AsyncResultData
        {
            public bool IsDone { get; set; }
            public bool HasError { get; set; }
            public string ErrorMessage { get; set; }
            public bool WasCancelled { get; set; }
            public float Percentage { get; set; }

            public AsyncResultData()
            {
            }

            public AsyncResultData(bool isDone, bool hasError, bool wasCancelled, float percentage)
            {
                IsDone = isDone;
                HasError = hasError;
                WasCancelled = wasCancelled;
                Percentage = percentage;
            }

            public virtual void Update(bool isDone, bool hasError, bool wasCancelled, float percentage)
            {
                IsDone = isDone;
                HasError = hasError;
                WasCancelled = wasCancelled;
                Percentage = percentage;
            }
        }

        public class CopyDirData : AsyncResultData
        {
            public string FileName { get; set; }
            public bool SuccessfullyCopied { get; set; }

            public CopyDirData()
            {
            }

            public CopyDirData(string fileName, bool successfullyCopied,
                bool isDone, float percentageDirCopied)
            {
                FileName = fileName;
                SuccessfullyCopied = successfullyCopied;
                IsDone = isDone;
                Percentage = percentageDirCopied;
            }

            public void Update(string fileName, bool successfullyCopied,
                bool isDone, float percentageDirCopied)
            {
                FileName = fileName;
                SuccessfullyCopied = successfullyCopied;
                IsDone = isDone;
                Percentage = percentageDirCopied;
            }
        }

        public class ScanDriveForFilesData : AsyncResultData
        {
            public string CurrentDirectory { get; set; }
            public List<string> FilesPath { get; set; }

            public ScanDriveForFilesData()
            {
                FilesPath = new List<string>();
            }
        }

        #endregion Data for Async operations

        public class FileWriteAsyncInfo
        {
            public FileStream Stream;

            public FileWriteAsyncInfo(FileStream stream)
            {
                Stream = stream;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class StorageInfo
        {
            private char letter;

            public string Letter
            {
                get { return letter + ":/"; }
            }

            private int type;

            public DriveType Type
            {
                get { return (DriveType)type; }
            }

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            private string deviceName;

            public string DeviceName
            {
                get { return deviceName; }
            }

            // In gb
            private float freeSpace;

            public float FreeSpace
            {
                get { return freeSpace; }
            }

            public StorageInfo()
            {
            }

            public StorageInfo(char letter, int type, string deviceName, float freeSpace)
            {
                this.letter = letter;
                this.type = type;
                this.deviceName = deviceName;
                this.freeSpace = freeSpace;
            }

            public StorageInfo(StorageInfo obj)
            {
                letter = obj.letter;
                type = obj.type;
                deviceName = obj.deviceName;
                freeSpace = obj.freeSpace;
            }
        };

        public enum SaveImageResults
        {
            SuccessfullySaved = 1,
            NotEnoughSpace = 2,
            FailedUnknowReason = 3
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa364939(v=vs.85).aspx
        /// </summary>
        public enum DriveType
        {
            DRIVE_UNKNOWN,
            DRIVE_NO_ROOT_DIR,
            DRIVE_REMOVABLE,
            DRIVE_FIXED,
            DRIVE_REMOTE,
            DRIVE_CDROM,
            DRIVE_RAMDISK
        }

        public enum SizeType
        {
            B = 1,
            KB = 1024,
            MB = 1048576,
            GB = 1073741824
        }

        [DllImport("PYUSBStorage")]
        private static extern int GetAmountDevices(int type);

        [DllImport("PYUSBStorage")]
        private static extern void GetStorageDevice(int index, int type, [In, Out] StorageInfo storage);

        private bool _cancelAsyncCopyDir = false;
        private List<string> _copiedFilesPath = new List<string>();

        // This actions is used to used all necessary callback in unitys mainthread
        private Queue<Action> _pendingActions = new Queue<Action>();

        private List<StorageInfo> _cachedStorageDevices = new List<StorageInfo>();

        #region Unity Functions

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void LateUpdate()
        {
            lock (_pendingActions) // Update all the time the pendingActions
            {
                if (_pendingActions.Count > 0)
                    _pendingActions.Dequeue()();
            }
        }

        #endregion Unity Functions

        public void StopAllRoutines()
        {
            StopAllCoroutines();
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        public List<StorageInfo> GetAllDevices()
        {
            int amountDevices = GetAmountDevices(ANY_TYPE_DEVICE);

            /// INFO: Apenas procura por devices caso algum
            /// algum novo device ou algum device
            /// tenha sido removido
            if (_cachedStorageDevices.Count != amountDevices)
            {
                _cachedStorageDevices.Clear();
                StorageInfo device = new StorageInfo();

                for (int x = 0; x < amountDevices; x++)
                {
                    GetStorageDevice(x, -1, device);
                    _cachedStorageDevices.Add(new StorageInfo(device));
                }
            }

            return _cachedStorageDevices;
        }

        public List<StorageInfo> GetAllDevices(DriveType type)
        {
            int typeInt = (int)type;
            int amountDevices = GetAmountDevices(ANY_TYPE_DEVICE);

            List<StorageInfo> devices = new List<StorageInfo>();

            if (_cachedStorageDevices.Count != amountDevices)
            {
                _cachedStorageDevices.Clear();
                StorageInfo device = new StorageInfo();

                for (int x = 0; x < amountDevices; x++)
                {
                    GetStorageDevice(x, typeInt, device);
                    _cachedStorageDevices.Add(new StorageInfo(device));

                    if (device.Type == type)
                        devices.Add(_cachedStorageDevices[_cachedStorageDevices.Count - 1]);
                }
            }
            else
            {
                for (int x = 0; x < amountDevices; x++)
                {
                    if (_cachedStorageDevices[x].Type == type)
                        devices.Add(_cachedStorageDevices[x]);
                }
            }

            return devices;
        }

        #region Obsolete

        [Obsolete("Use the method GetDevice(char letter)")]
        public StorageInfo GetUSBDevice(char letter)
        {
            foreach (StorageInfo dev in GetAllDevices(DriveType.DRIVE_REMOVABLE))
            {
                if (dev.Letter.Equals(letter))
                    return dev;
            }
            return null;
        }

        [Obsolete("Use the method GetDevice(string deviceName)")]
        public StorageInfo GetUSBDevice(string deviceName)
        {
            foreach (StorageInfo dev in GetAllDevices(DriveType.DRIVE_REMOVABLE))
            {
                if (dev.DeviceName.Equals(deviceName))
                    return dev;
            }
            return null;
        }

        #endregion Obsolete

        public StorageInfo GetDevice(char letter)
        {
            foreach (StorageInfo dev in GetAllDevices())
                if (dev.Letter.Equals(letter))
                    return dev;
            return null;
        }

        public StorageInfo GetDevice(string deviceName)
        {
            foreach (StorageInfo dev in GetAllDevices())
                if (dev.DeviceName.Equals(deviceName))
                    return dev;
            return null;
        }

        public StorageInfo GetDevice(DriveType type)
        {
            List<StorageInfo> devices = GetAllDevices(type);
            return devices.Count > 0 ? devices[0] : null;
        }

        public bool HasDevices(char letter)
        {
            return GetDevice(letter) != null;
        }

        public bool HasDevices(string deviceName)
        {
            return GetDevice(deviceName) != null;
        }

        public bool HasDevices(DriveType type)
        {
            if (GetDevice(type) != null)
                return true;

            return GetAmountDevices((int)type) != 0;
        }

        private bool isWaitingForDevices = false;

        public void StopWaitingForDevices()
        {
            isWaitingForDevices = false;
        }

        public void AsyncScanForDevices(DriveType type, Action callback)
        {
            isWaitingForDevices = true;
            StartCoroutine(ScanForDevicesRoutine(type, callback));
        }

        private System.Collections.IEnumerator ScanForDevicesRoutine(DriveType type, Action callback)
        {
            while (!HasDevices(type) && isWaitingForDevices)
                yield return new WaitForSeconds(5);

            if (isWaitingForDevices && callback != null)
                Invoke(callback);

            isWaitingForDevices = false;
        }

        /// <summary>
        /// Save the image in the specified device.
        /// </summary>
        /// <param name="usbDevice">USB device got from GetUSBDevice</param>
        /// <param name="path">Path where the file will be saved without the filename</param>
        /// <param name="fileName">File name without extension</param>
        /// <param name="img">Texture2D you want to save</param>
        /// <param name="resultCallback">
        /// Result code:
        /// 1: SuccessfullySaved
        /// 2: NotEnoughSpace
        /// 3: FailedUnknowReason
        /// </param>
        public void SaveImage(StorageInfo usbDevice, string path, string fileName, Texture2D img, Action<SaveImageResults> resultCallback)
        {
            if (usbDevice == null || fileName == null || img == null)
                if (resultCallback != null)
                    Invoke(() => resultCallback(SaveImageResults.FailedUnknowReason));

            path = FilterPath(usbDevice.Letter + path);
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                byte[] imgBytes = img.EncodeToPNG();

                // Img size in Kb
                float imgSize = (sizeof(byte) * imgBytes.Length) / 1000;
                // Img size in Mb
                imgSize /= 1024;
                // Img size in Gb
                imgSize /= 1024;

                if (imgSize > usbDevice.FreeSpace)
                {
                    if (resultCallback != null)
                        Invoke(() => resultCallback(SaveImageResults.NotEnoughSpace));
                }
                else
                    AsyncWriteFile(path + fileName + ".png", imgBytes, false, resultCallback);
            }
            catch
            {
                if (resultCallback != null)
                    Invoke(() => resultCallback(SaveImageResults.FailedUnknowReason));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path">Path where the file will be saved without the filename</param>
        /// <param name="fileName">File name without extension</param>
        /// <param name="img">Texture2D you want to save</param>
        /// <param name="overwrite">If true the file will be overwritten, otherwise will be
        /// added in the end of the file name a index</param>
        /// <param name="resultCallback">
        /// Result code:
        /// 1: SuccessfullySaved
        /// 2: NotEnoughSpace
        /// 3: FailedUnknowReason
        /// </param>
        public void SaveImage(string path, string fileName, Texture2D img, bool overwrite, Action<SaveImageResults> resultCallback)
        {
            try
            {
                path = FilterPath(path);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                AsyncWriteFile(path + fileName + ".png", img.EncodeToPNG(), overwrite, resultCallback);
            }
            catch
            {
                if (resultCallback != null)
                    Invoke(() => resultCallback(SaveImageResults.FailedUnknowReason));
            }
        }

        /// <summary>
        /// Saves a image with a automatic create thumbnail.
        /// The thumbnail will always have the name of the file with a prefix
        /// of thumbnail_, example thumbnail_iconPlayer and its size will be 64x64,
        /// it will be saved together with the file
        /// </summary>
        /// <param name="path">Path where the file will be saved without the filename</param>
        /// <param name="fileName">File name without extension</param>
        /// <param name="img">Texture2D you want to save</param>
        /// <param name="overwrite">If true the file will be overwritten, otherwise will be
        /// added in the end of the file name a index</param>
        /// <param name="resultCallback">
        /// Result code:
        /// 1: SuccessfullySaved
        /// 2: NotEnoughSpace
        /// 3: FailedUnknowReason
        /// </param>
        public void SaveImageWithThumbnail(string path, string fileName, Texture2D img, bool overwrite, Action<SaveImageResults> resultCallback)
        {
            try
            {
                // Creates the thumbnail
                Texture2D thumbnailImg = new Texture2D(img.width, img.height, img.format, false);
                thumbnailImg.SetPixels32(img.GetPixels32());
                thumbnailImg.Apply(false, false);
                //TextureExtension.ScaleBilinear(thumbnailImg, 64, 64);

                SaveImage(path, "thumbnail_" + fileName, thumbnailImg, overwrite, (thumbData) =>
                {
                    thumbnailImg = null;
                    // If has happened any error during the thumbnail creation we
                    // call the callback
                    if (thumbData != SaveImageResults.SuccessfullySaved)
                    {
                        if (resultCallback != null)
                            resultCallback(thumbData);
                    }
                    else
                    {
                        SaveImage(path, fileName, img, overwrite, (imgData) =>
                        {
                            Resources.UnloadUnusedAssets();
                            if (resultCallback != null)
                                resultCallback(imgData);
                        });
                    }
                });
            }
            catch
            {
                if (resultCallback != null)
                    resultCallback(SaveImageResults.FailedUnknowReason);
            }
        }

        /// <summary>
        /// Write any file using async operation
        /// </summary>
        /// <param name="fullPath">Fullpath of the file including its name and extension</param>
        /// <param name="fileBytes">File bytes that will be written</param>
        /// <param name="overwrite">If true the file will be overwritten, otherwise will be
        /// added in the end of the file name a index</param>
        /// <param name="resultCallback">
        /// Result code:
        /// 1: SuccessfullySaved
        /// 2: NotEnoughSpace
        /// 3: FailedUnknowReason
        /// </param>
        public void AsyncWriteFile(string fullPath, byte[] fileBytes, bool overwrite, Action<SaveImageResults> resultCallback)
        {
            FileInfo fileInfo = new FileInfo(fullPath);
            string fileName = fileInfo.Name.Split('.')[0];
            string extension = "." + fileInfo.Name.Split('.')[1];
            string absolutePath = fileInfo.FullName;

            // If we dont wanna overwrite the file we add a index in its name
            if (!overwrite)
            {
                // Add a index to the img name if the name matchs a already created one
                int counter = 1;
                try
                {
                    while (File.Exists(absolutePath))
                    {
                        absolutePath = FilterPath(fileInfo.DirectoryName) + fileName + string.Format("({0}){1}", counter, extension);
                        counter++;
                    }
                }
                catch
                {
                    if (resultCallback != null)
                        Invoke(() => resultCallback(SaveImageResults.FailedUnknowReason));
                }
            }

            FileStream stream = null;
            try
            {
                // Create the directory struct if necessary
                if (!Directory.Exists(fileInfo.DirectoryName))
                    Directory.CreateDirectory(fileInfo.DirectoryName);

                // Write bytes to file
                stream = new FileStream(absolutePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, fileBytes.Length);
                stream.BeginWrite(fileBytes, 0, fileBytes.Length, new AsyncCallback((result) =>
                {
                    FileWriteAsyncInfo info = (FileWriteAsyncInfo)result.AsyncState;

                    try
                    {
                        info.Stream.EndWrite(result);

                        if (File.Exists(absolutePath))
                        {
                            if (resultCallback != null)
                                Invoke(() => resultCallback(SaveImageResults.SuccessfullySaved));
                        }
                        else if (resultCallback != null)
                            Invoke(() => resultCallback(SaveImageResults.FailedUnknowReason));
                    }
                    catch
                    {
                        if (resultCallback != null)
                            Invoke(() => resultCallback(SaveImageResults.FailedUnknowReason));
                    }

                    info.Stream.Close();
                }), new FileWriteAsyncInfo(stream));
            }
            catch
            {
                if (stream != null)
                    stream.Close();

                if (resultCallback != null)
                    Invoke(() => resultCallback(SaveImageResults.FailedUnknowReason));
            }
        }

        public void AsyncLoadImagesFromPath(string path, int startImageIndex, int amountToLoad, string filesExtension = "*.png", Action<Texture2D[]> onLoadedAll = null)
        {
            StartCoroutine(asyncLoadImagesFromPath(path, startImageIndex, amountToLoad, filesExtension, onLoadedAll));
        }

        public void AsyncLoadImagesFromPath(string path, string filesExtension = "*.png", Action<Texture2D[]> onLoadedAll = null)
        {
            StartCoroutine(asyncLoadImagesFromPath(path, 0, 0, filesExtension, onLoadedAll));
        }

        private System.Collections.IEnumerator asyncLoadImagesFromPath(string path, int startImageIndex, int amountToLoad, string filesExtension, Action<Texture2D[]> onLoadedAll)
        {
            List<Texture2D> imgs = new List<Texture2D>();

            if (path[path.Length - 1] != '/')
                path += "/";

            if (Directory.Exists(path))
            {
                Debug.Log(path);
                List<string> allImages = new List<string>(Directory.GetFiles(path, filesExtension));
                allImages.Sort();

                // This is to filter from where the images will be loaded, if less than 0
                // will load from begging
                if (startImageIndex > 0)
                    allImages.RemoveRange(0, startImageIndex);

                // Limit the amount of images that will be loaded, if less than 0
                // will load all images
                if (amountToLoad > 0 && allImages.Count > amountToLoad)
                    allImages.RemoveRange(amountToLoad, Mathf.Min(allImages.Count - amountToLoad, allImages.Count));

                foreach (string imgPath in allImages)
                {
                    Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    tex.name = GetFileName(imgPath, false);
                    tex.filterMode = FilterMode.Trilinear;
                    tex.anisoLevel = 1;
                    if (tex.LoadImage(File.ReadAllBytes(imgPath)))
                        imgs.Add(tex);

                    yield return null;
                }
            }

            if (onLoadedAll != null)
                Invoke(() => onLoadedAll(imgs.ToArray()));
        }

        /// <summary>
        /// Load a image from a path on disc
        /// </summary>
        /// <param name="path">full path with name and extension</param>
        /// <param name="compress">from 0 to 1: the percetage of resize</param>
        /// <param name="callback">return the sprite</param>
        public void AsyncLoadSpriteFromPath(string path, float compress, Action<Sprite> callback)
        {
            AsyncLoadImageFromPath(path,
                (texture) =>
                {
                    if (callback != null)
                    {
                        if (texture != null)
                        {
                            if (texture.width > 1 && texture.height > 1)
                                TextureScaler.scale(texture, (int)(texture.width * compress), (int)(texture.height * compress), FilterMode.Bilinear);

                            callback(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2));
                        }
                        else
                            //TOPO: VER DEPOIS;
                            callback(null);//new Sprite());
                    }
                    texture = null;
                });
        }

        public void AsyncLoadSpriteFromPath(string path, int height, int width, Action<Sprite> callback)
        {
            AsyncLoadImageFromPath(path,
                (texture) =>
                {
                    if (callback != null)
                    {
                        if (texture != null && texture.width > 1 && texture.height > 1)
                            TextureScaler.scale(texture, width, height, FilterMode.Bilinear);
                        callback(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2));
                    }
                    texture = null;
                });
        }

        public void AsyncLoadSpriteFromPath(string path, Action<Sprite> callback)
        {
            AsyncLoadImageFromPath(path,
                (texture) =>
                {
                    if (callback != null)
                        callback(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2));
                    texture = null;
                });
        }

        public void AsyncLoadImageFromPath(string path, Action<Texture2D> onLoaded)
        {
            StartCoroutine(asyncLoadImageFromPath(path, onLoaded));
        }

        public System.Collections.IEnumerator TrueAsyncLoadSprite(string path, Action<Sprite> callback, int height = 0, int width = 0)
        {
            yield return asyncLoadImageFromPath(path, (texture) =>
            {
                if (texture != null && texture.width > 1 && texture.height > 1 && (height > 0 && width > 0))
                    TextureScaler.scale(texture, width, height, FilterMode.Bilinear);
                callback(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2));
            });
        }

        private System.Collections.IEnumerator asyncLoadImageFromPath(string path, Action<Texture2D> onLoaded)
        {
            WWW www = new WWW("file:///" + path);
            yield return www;

            Texture2D tex;
            if (string.IsNullOrEmpty(www.error))
            {
                tex = www.texture;
                tex.name = GetFileName(path, false);
                tex.filterMode = FilterMode.Trilinear;

                if (onLoaded != null)
                    Invoke(() => onLoaded(tex));
            }
            else
            {
                Debug.LogError("Error reading image at path: " + path
                    + " Error Message: " + www.error);
                if (onLoaded != null)
                    Invoke(() => onLoaded(null));
            }
        }

        public void AsyncCopyFile(string fullPath, string dstPath, Action<CopyDirData> resultCallback)
        {
            string fileName = new FileInfo(dstPath).Name;
            //fullPath = FilterPath(fullPath);
            //dstPath = FilterPath(dstPath);
            dstPath = dstPath.Replace("/", @"\");
            fullPath = fullPath.Replace("/", @"\");

            if (!File.Exists(fullPath))
            {
                if (resultCallback != null)
                    Invoke(() => resultCallback(new CopyDirData(fileName, false, true, 0)));
                return;
            }

            using (FileStream sourceStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] srcBuffer = new byte[sourceStream.Length];
                try
                {
                    sourceStream.Read(srcBuffer, 0, srcBuffer.Length);

                    if (!Directory.Exists(new FileInfo(dstPath).Directory.FullName))
                        Directory.CreateDirectory(new FileInfo(dstPath).Directory.FullName);
                }
                catch
                {
                    if (resultCallback != null)
                        Invoke(() => resultCallback(new CopyDirData(fileName, false, true, 0)));
                    return;
                }

                FileStream destStream = new FileStream(dstPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                destStream.BeginWrite(srcBuffer, 0, srcBuffer.Length, new AsyncCallback((result) =>
                {
                    FileWriteAsyncInfo info = (FileWriteAsyncInfo)result.AsyncState;

                    try
                    {
                        info.Stream.EndWrite(result);

                        if (File.Exists(dstPath))
                        {
                            if (resultCallback != null)
                                Invoke(() => resultCallback(new CopyDirData(fileName, true, true, 1)));
                        }
                        else if (resultCallback != null)
                            Invoke(() => resultCallback(new CopyDirData(fileName, false, true, 0)));
                    }
                    catch
                    {
                        if (resultCallback != null)
                            Invoke(() => resultCallback(new CopyDirData(fileName, false, true, 0)));
                    }

                    info.Stream.Close();
                }), new FileWriteAsyncInfo(destStream));
            }
        }

        public void AsyncCopyListFiles(string[] srcsPath, string destPath, Action<CopyDirData> resultCallback)
        {
            _copiedFilesPath.Clear();

            // Construct a list with a path for every file
            string[] destFilesPath = new string[srcsPath.Length];
            for (int i = 0; i < srcsPath.Length; i++)
                destFilesPath[i] = destPath + GetFileName(srcsPath[i]);

            StartCoroutine(CopyListFilesRoutine(srcsPath, destFilesPath, resultCallback));
        }

        public void AsyncCopyListFiles(string[] srcsPath, string[] destsPath, Action<CopyDirData> resultCallback)
        {
            _copiedFilesPath.Clear();
            StartCoroutine(CopyListFilesRoutine(srcsPath, destsPath, resultCallback));
        }

        private System.Collections.IEnumerator CopyListFilesRoutine(string[] srcDirPath, string[] destDirPath, Action<CopyDirData> resultCallback)
        {
            bool hasError = false;
            CopyDirData resultData = new CopyDirData();

            for (int i = 0; i < srcDirPath.Length; i++)
            {
                if (_cancelAsyncCopyDir || hasError)
                    break;

                try
                {
                    FileInfo fileInfo = new FileInfo(destDirPath[i]);
                    if (!Directory.Exists(fileInfo.Directory.FullName))
                        Directory.CreateDirectory(fileInfo.Directory.FullName);

                    File.Copy(srcDirPath[i], destDirPath[i], true);

                    // Make sure the file was copied to the dest path
                    if (File.Exists(destDirPath[i]))
                    {
                        resultData.Update(fileInfo.Name.Split('.')[0], true, i == (srcDirPath.Length - 1), (float)i / (srcDirPath.Length - 1));
                        _copiedFilesPath.Add(destDirPath[i]);
                    }
                    else
                        resultData.Update(fileInfo.Name.Split('.')[0], false, i == (srcDirPath.Length - 1), (float)i / (srcDirPath.Length - 1));
                }
                catch (Exception e)
                {
                    resultData.Update(e.Message, false, true, -1);
                    hasError = true;
                    break;
                }

                if (resultData.IsDone)
                    yield return new WaitForSeconds(1);
                else
                    yield return null;

                if (resultCallback != null)
                    Invoke(() => resultCallback(resultData));
            }

            if (_cancelAsyncCopyDir || hasError)
            {
                resultData.IsDone = true;
                resultData.SuccessfullyCopied = false;
                resultData.HasError = hasError;
                resultData.WasCancelled = _cancelAsyncCopyDir;
                if (resultCallback != null)
                    Invoke(() => resultCallback(resultData));

                _cancelAsyncCopyDir = false;
            }
        }

        public void AsyncCopyDir(string srcDirPath, string destDirPath, Action<CopyDirData> resultCallback)
        {
            _copiedFilesPath.Clear();
            StartCoroutine(AsyncCopyDirRoutine(srcDirPath, destDirPath, resultCallback));
        }

        private System.Collections.IEnumerator AsyncCopyDirRoutine(string srcDirPath, string destDirPath, Action<CopyDirData> resultCallback)
        {
            srcDirPath = FilterPath(srcDirPath);
            destDirPath = FilterPath(destDirPath);

            bool hasError = false;

            float currentFileCopied = 0;
            string[] filesPath = Directory.GetFiles(srcDirPath, "*.*", SearchOption.AllDirectories);

            CopyDirData resultData = new CopyDirData();
            for (int x = 0; x < filesPath.Length; x++)
            {
                string filePath = filesPath[x];
                string fileName = GetFileName(filePath);

                if (_cancelAsyncCopyDir || hasError)
                    break;

                try
                {
                    using (FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        string fileRelativePath = filePath.Replace(srcDirPath, "");

                        byte[] srcBuffer = new byte[sourceStream.Length];
                        try
                        {
                            sourceStream.Read(srcBuffer, 0, srcBuffer.Length);

                            DirectoryInfo dirInfo = Directory.GetParent(destDirPath + fileRelativePath);
                            if (!dirInfo.Exists)
                                Directory.CreateDirectory(dirInfo.FullName);
                        }
                        catch
                        {
                            resultData.Update(fileName, false, true, currentFileCopied / filesPath.Length);
                            hasError = true;
                            break;
                        }

                        if (!hasError)
                        {
                            FileStream destStream = new FileStream(destDirPath + fileRelativePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                            destStream.BeginWrite(srcBuffer, 0, srcBuffer.Length, new AsyncCallback((result) =>
                            {
                                FileWriteAsyncInfo info = (FileWriteAsyncInfo)result.AsyncState;

                                try
                                {
                                    info.Stream.EndWrite(result);
                                    currentFileCopied++;

                                    if (File.Exists(destDirPath + fileRelativePath))
                                    {
                                        _copiedFilesPath.Add(destDirPath + fileRelativePath);
                                        resultData.Update(GetFileName(fileRelativePath), true, currentFileCopied == filesPath.Length, currentFileCopied / filesPath.Length);
                                    }
                                    else
                                        resultData.Update(GetFileName(fileRelativePath), false, currentFileCopied == filesPath.Length, currentFileCopied / filesPath.Length);
                                }
                                catch
                                {
                                    resultData.Update(GetFileName(fileRelativePath), false, currentFileCopied == filesPath.Length, currentFileCopied / filesPath.Length);
                                    hasError = true;
                                }

                                info.Stream.Close();

                                // Send callback
                                if (resultCallback != null)
                                    Invoke(() => resultCallback(resultData));
                            }), new FileWriteAsyncInfo(destStream));
                        }
                    }

                    if (hasError)
                        break;
                }
                catch
                {
                    resultData.Update("???", false, true, 0);
                    hasError = true;

                    // Send callback
                    if (resultCallback != null)
                        Invoke(() => resultCallback(resultData));
                    break;
                }

                yield return new WaitForSeconds(0.1f);
                yield return new WaitForEndOfFrame();
            }

            if (_cancelAsyncCopyDir || hasError)
            {
                resultData.IsDone = false;
                resultData.SuccessfullyCopied = false;
                resultData.HasError = hasError;
                resultData.WasCancelled = _cancelAsyncCopyDir;
                if (resultCallback != null)
                    Invoke(() => resultCallback(resultData));

                _cancelAsyncCopyDir = false;
            }
        }

        public void CancelAsyncCopyDir()
        {
            _cancelAsyncCopyDir = true;
        }

        /// <summary>
        /// This methods should be used just after any AsyncCopyDir methods
        /// </summary>
        public void DeleteAllFilesCopied()
        {
            if (_copiedFilesPath.Count > 0)
                StartCoroutine(DeleteAllFilesCopiedRoutine());
        }

        private System.Collections.IEnumerator DeleteAllFilesCopiedRoutine()
        {
            foreach (string filePath in _copiedFilesPath)
            {
                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch { }

                yield return new WaitForEndOfFrame();
            }
        }

        public bool DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                return true;
            }
            catch
            {
                Debug.Log("Error while deleting folder at: " + path);
                return false;
            }
        }

        public int GetAmountFilesInDir(string dir)
        {
            return GetAmountFilesInDir(dir, "*.*");
        }

        public int GetAmountFilesInDir(string dir, string fileExtension)
        {
            dir = FilterPath(dir);
            try
            {
                if (Directory.Exists(dir))
                    return Directory.GetFiles(dir, fileExtension, SearchOption.AllDirectories).Length;
                else
                    return -1;
            }
            catch
            {
                return -1;
            }
        }

        public string[] GetAllFilesPathInDir(string dir, string fileExtension)
        {
            dir = FilterPath(dir);
            try
            {
                if (Directory.Exists(dir))
                    return Directory.GetFiles(dir, fileExtension, SearchOption.AllDirectories);
                else
                    return new string[0];
            }
            catch
            {
                return new string[0];
            }
        }

        public void AsyncScanPathForFiles(string rootDir, string[] fileExtensions,
            Action<ScanDriveForFilesData> resultCallback)
        {
            _cancelAsyncCopyDir = false;
            StartCoroutine(ScanPathForFilesRoutine(rootDir, fileExtensions, resultCallback));
        }

        private System.Collections.IEnumerator ScanPathForFilesRoutine(string rootDir, string[] fileExtensions,
            Action<ScanDriveForFilesData> resultCallback)
        {
            DirectoryInfo rootDirInfo = new DirectoryInfo(rootDir);
            Queue<DirectoryInfo> dirs = new Queue<DirectoryInfo>();
            ScanDriveForFilesData data = new ScanDriveForFilesData();

            try
            {
                dirs.Enqueue(rootDirInfo);
                foreach (DirectoryInfo dir in rootDirInfo.GetDirectories())
                    dirs.Enqueue(dir);
            }
            catch (Exception e)
            {
                data.CurrentDirectory = rootDir;
                data.HasError = true;
                data.ErrorMessage = e.Message;
                if (resultCallback != null)
                    resultCallback(data);
            }

            if (!data.HasError)
            {
                float maxAmountDirs = dirs.Count;
                float currentAmountDirsVisited = 0;
                while (dirs.Count > 0 && !_cancelAsyncCopyDir)
                {
                    try
                    {
                        DirectoryInfo current = dirs.Dequeue();
                        if ((current.Attributes & FileAttributes.System) != FileAttributes.System)
                        {
                            for (int x = 0; x < fileExtensions.Length; x++)
                            {
                                foreach (FileInfo file in current.GetFiles(fileExtensions[x]))
                                    data.FilesPath.Add(file.FullName);
                            }

                            float temp = dirs.Count;
                            foreach (DirectoryInfo dir in current.GetDirectories())
                                dirs.Enqueue(dir);

                            maxAmountDirs += dirs.Count - temp;
                        }

                        data.FilesPath = data.FilesPath.Distinct().ToList();
                        currentAmountDirsVisited++;

                        data.CurrentDirectory = current.FullName;
                        data.Percentage = currentAmountDirsVisited / maxAmountDirs;
                        if (resultCallback != null)
                            resultCallback(data);
                    }
                    catch (Exception e)
                    {
                        data.HasError = true;
                        data.ErrorMessage = e.Message;
                        data.IsDone = false;
                        break;
                    }

                    if (dirs.Count == 0)
                        yield return new WaitForSeconds(1);

                    yield return null;
                }

                data.IsDone = dirs.Count == 0;

                if (_cancelAsyncCopyDir)
                {
                    data.FilesPath.Clear();
                    data.IsDone = false;
                    data.WasCancelled = true;
                }

                if (resultCallback != null)
                    resultCallback(data);
            }
        }

        public float GetDirectorySize(string dir, SizeType type = SizeType.B)
        {
            dir = FilterPath(dir);
            try
            {
                string[] filesPath = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

                float fileSize = 0;
                foreach (string name in filesPath)
                {
                    FileInfo info = new FileInfo(name);
                    fileSize += info.Length;
                    fileSize += info.FullName.Length;
                }

                return fileSize / (float)type;
            }
            catch
            {
                return 0;
            }
        }

        public float ConvertByteSizeTo(float size, SizeType type)
        {
            return size / (float)type;
        }

        public string RemoveSpecialChars(string input)
        {
            return Regex.Replace(input, @"[^0-9a-zA-Z_/]", " ");
        }

        public static string GetFileName(string path, bool withExtension = true)
        {
            string[] split = path.Split('\\');

            if (withExtension)
                return split[split.Length - 1];
            else
                return split[split.Length - 1].Split('.')[0];
        }

        private string FilterPath(string path)
        {
            if (path == null)
                path = "";
            if (path.Length > 1)
                path = path[path.Length - 1] != '/' ? path + "/" : path;

            return path.Replace("/", @"\");
        }

        private void Invoke(Action fn)
        {
            lock (_pendingActions)
            {
                _pendingActions.Enqueue(fn);
                //_pendingActions.Add(fn);
            }
        }
    }
}