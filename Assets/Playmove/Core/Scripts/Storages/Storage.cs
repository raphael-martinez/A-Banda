using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Playmove.Core.Storages
{
    /// <summary>
    /// Informations about pendrive
    /// </summary>
    public class PendriveInfo
    {
        public string Name { get; private set; }
        public long AvailableSpace { get; private set; }
        public long TotalSpace { get; private set; }
        public string RootDirectory { get; private set; }
        public bool IsReady { get; private set; }

        public PendriveInfo(DriveInfo drive)
        {
            Name = drive.Name;
            AvailableSpace = drive.AvailableFreeSpace;
            TotalSpace = drive.TotalSize;
            RootDirectory = drive.RootDirectory.FullName;
            IsReady = drive.IsReady;
        }

        public float GetAvailableSpace(SizeFormat format)
        {
            return AvailableSpace / (float)format;
        }
        public float GetTotalSpace(SizeFormat format)
        {
            return TotalSpace / (float)format;
        }

        public List<string> GetFiles(SearchOption searchOption, params string[] searchPatterns)
        {
            return Storage.GetFilesPathAt(RootDirectory, searchOption, searchPatterns);
        }
        public List<string> GetFiles(string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Storage.GetFilesPathAt(RootDirectory, searchPattern, searchOption);
        }
    }

    /// <summary>
    /// Responsible to work with pendrive, folders and files
    /// </summary>
    public static class Storage
    {
        /// <summary>
        /// Event raised when a pendrive is found
        /// </summary>
        public static event Action<PendriveInfo> OnFoundPendrive;
        /// <summary>
        /// Event raised when a pendrive is disconnected
        /// </summary>
        public static event Action OnDisconnectedPendrive;

        /// <summary>
        /// Current connected pendrive
        /// </summary>
        public static PendriveInfo ConnectedPendrive { get; private set; }
        
        /// <summary>
        /// This will start the scan for a pendrive and will wait until one is connected
        /// or user decides to cancel it
        /// </summary>
        /// <param name="foundCallback"></param>
        public static void ScanForPendrive(AsyncCallback<PendriveInfo> foundCallback)
        {
            _shouldCancelPendriveScan = false;
            Playtable.Instance.StartCoroutine(ScanForPendriveRoutine(foundCallback));
        }
        private static IEnumerator ScanForPendriveRoutine(AsyncCallback<PendriveInfo> foundCallback)
        {
            if (ConnectedPendrive == null)
            {
                DriveInfo pendrive = null;
                while (pendrive == null && !_shouldCancelPendriveScan)
                {
                    try
                    {
                        pendrive = DriveInfo.GetDrives().FirstOrDefault(drive => drive.DriveType == DriveType.Removable);
                        ConnectedPendrive = new PendriveInfo(pendrive);
                    }
                    catch
                    {
                        pendrive = null;
                    }
                    yield return new WaitForSeconds(3);
                }

                if (!_shouldCancelPendriveScan)
                {
                    foundCallback?.Invoke(new AsyncResult<PendriveInfo>(ConnectedPendrive, string.Empty));
                    OnFoundPendrive?.Invoke(ConnectedPendrive);

                    WatchPendriveConnection();
                }

                _shouldCancelPendriveScan = false;
            }
            else
                foundCallback?.Invoke(new AsyncResult<PendriveInfo>(ConnectedPendrive, string.Empty));
        }

        private static bool _shouldCancelPendriveScan = false;
        /// <summary>
        /// Cancel the pendrive scan when you want
        /// </summary>
        public static void CancelPendriveScan()
        {
            _shouldCancelPendriveScan = true;
        }

        public static void LoadSprites(List<string> fullpaths, AsyncCallback<List<Sprite>> completed, Action<float> progress = null)
        {
            LoadSprites(fullpaths, 0, 0, false, completed, progress);
        }
        public static void LoadSprites(List<string> fullpaths, int width, int height, bool keepAspect, AsyncCallback<List<Sprite>> completed, Action<float> progress = null)
        {
            LoadSpritesAsync(fullpaths, width, height, keepAspect, progress).ContinueWith(result => completed?.Invoke(result.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<List<Sprite>>> LoadSpritesAsync(List<string> fullpaths, int width, int height, bool keepAspect, Action<float> progress = null)
        {
            List<Sprite> files = new List<Sprite>();
            List<Task> tasks = new List<Task>();
            float loadedFiles = 0;
            foreach (string fullpath in fullpaths)
            {
                tasks.Add(LoadSpriteAsync(fullpath, width, height, keepAspect).ContinueWith(result =>
                {
                    if (!result.Result.HasError) files.Add(result.Result.Data);
                    progress?.Invoke(loadedFiles / fullpaths.Count);
                    loadedFiles++;
                }));
            }
            await Task.WhenAll(tasks);
            progress?.Invoke(loadedFiles / fullpaths.Count);
            return new AsyncResult<List<Sprite>>(files, string.Empty);
        }
        /// <summary>
        /// Load a list of textures, if any of the files fail it will be ignored and not loaded
        /// so the amount of textures loaded can be different from the requested ones
        /// </summary>
        /// <param name="fullpaths">Paths to files</param>
        /// <param name="completed">Callback when all files have being loaded</param>
        /// <param name="progress">Callback when any file is loaded to indicate the progress</param>
        public static void LoadTextures2D(List<string> fullpaths, AsyncCallback<List<Texture2D>> completed, Action<float> progress = null)
        {
            LoadTextures2D(fullpaths, 0, 0, false, completed, progress);
        }
        public static void LoadTextures2D(List<string> fullpaths, int width, int height, bool keepAspect, AsyncCallback<List<Texture2D>> completed, Action<float> progress = null)
        {
            LoadTextures2DAsync(fullpaths, width, height, keepAspect, progress).ContinueWith(result => completed?.Invoke(result.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<List<Texture2D>>> LoadTextures2DAsync(List<string> fullpaths, int width, int height, bool keepAspect, Action<float> progress = null)
        {
            List<Texture2D> files = new List<Texture2D>();
            List<Task> tasks = new List<Task>();
            float loadedFiles = 0;
            foreach (string fullpath in fullpaths)
            {
                tasks.Add(LoadTexture2DAsync(fullpath, width, height, keepAspect).ContinueWith(result =>
                {
                    if (!result.Result.HasError) files.Add(result.Result.Data);
                    progress?.Invoke(loadedFiles / fullpaths.Count);
                    loadedFiles++;
                }));
            }
            await Task.WhenAll(tasks);
            progress?.Invoke(loadedFiles / fullpaths.Count);
            return new AsyncResult<List<Texture2D>>(files, string.Empty);
        }

        /// <summary>
        /// Load Sprite from specified path
        /// </summary>
        /// <param name="fullpath">Path to file</param>
        /// <param name="completed">Callback when the sprite is loaded</param>
        public static void LoadSprite(string fullpath, AsyncCallback<Sprite> completed)
        {
            LoadSprite(fullpath, 0, 0, false, completed);
        }
        public static void LoadSprite(string fullpath, int width, int height, bool keepAspect, AsyncCallback<Sprite> completed)
        {
            LoadSpriteAsync(fullpath, width, height, keepAspect).ContinueWith(result => completed?.Invoke(result.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<Sprite>> LoadSpriteAsync(string fullpath, int width, int height, bool keepAspect)
        {
            AsyncResult<Texture2D> textureResult = await LoadTexture2DAsync(fullpath, width, height, keepAspect);
            if (textureResult.HasError)
                return new AsyncResult<Sprite>(null, textureResult.Error);

            Sprite sprite = Sprite.Create(textureResult.Data, new Rect(0, 0, textureResult.Data.width, textureResult.Data.height), Vector2.one / 2,
                100, 1, SpriteMeshType.FullRect, Vector4.one, false);
            sprite.name = textureResult.Data.name;
            if (sprite != null)
                return new AsyncResult<Sprite>(sprite, string.Empty);
            else
                return new AsyncResult<Sprite>(null, $"Failed to load sprite {fullpath}");
        }
        /// <summary>
        /// Load Texture2D from specified path
        /// </summary>
        /// <param name="fullpath">Path to file</param>
        /// <param name="completed">Callback when the texture2D is loaded</param>
        public static void LoadTexture2D(string fullpath, AsyncCallback<Texture2D> completed)
        {
            LoadTexture2D(fullpath, 0, 0, false, completed);
        }
        public static void LoadTexture2D(string fullpath, int width, int height, bool keepAspect, AsyncCallback<Texture2D> completed)
        {
            LoadTexture2DAsync(fullpath, width, height, keepAspect).ContinueWith(result => completed?.Invoke(result.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<Texture2D>> LoadTexture2DAsync(string fullpath, int width, int height, bool keepAspect)
        {
            AsyncResult<byte[]> fileResult = await ReadFileAsync(fullpath);
            if (fileResult.HasError)
                return new AsyncResult<Texture2D>(null, fileResult.Error);

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, true)
            {
                name = Path.GetFileNameWithoutExtension(fullpath)
            };
            bool loaded = texture.LoadImage(fileResult.Data);
            if ((width > 0 || height > 0) && (texture.width != width || texture.height != height))
            {
                width = width > 0 ? width : texture.width;
                height = height > 0 ? height : texture.height;
                texture.Scale(width, height, keepAspect);
            }

            if (loaded)
                return new AsyncResult<Texture2D>(texture, string.Empty);
            else
                return new AsyncResult<Texture2D>(null, $"Failed to load texture2D {fullpath}");
        }

        /// <summary>
        /// Save sprite at specified path, if overwrite is false a new file will be created
        /// with a number to indicates the new file
        /// </summary>
        /// <param name="fullpath">Path to save file</param>
        /// <param name="sprite">Sprite to be saved</param>
        /// <param name="overwrite">Overwrite the file or not</param>
        /// <param name="completed">Callback when the file is saved</param>
        public static void SaveSprite(string fullpath, Sprite sprite, bool overwrite, AsyncCallback<bool> completed)
        {
            SaveSpriteAsync(fullpath, sprite, overwrite).ContinueWith(result => completed?.Invoke(result.Result), 
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<bool>> SaveSpriteAsync(string fullpath, Sprite sprite, bool overwrite)
        {
            return await WriteFileAsync(fullpath, sprite.texture.EncodeToPNG(), overwrite);
        }
        /// <summary>
        /// Save texture2D at specified path, if overwrite is false a new file will be created
        /// with a number to indicates the new file
        /// </summary>
        /// <param name="fullpath">Path to save file</param>
        /// <param name="texture">Texture2D to be saved</param>
        /// <param name="overwrite">Overwrite the file or not</param>
        /// <param name="completed">Callback when the file is saved</param>
        public static void SaveTexture2D(string fullpath, Texture2D texture, bool overwrite, AsyncCallback<bool> completed)
        {
            SaveTexture2DAsync(fullpath, texture, overwrite).ContinueWith(result => completed?.Invoke(result.Result), 
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<bool>> SaveTexture2DAsync(string fullpath, Texture2D texture, bool overwrite)
        {
            return await WriteFileAsync(fullpath, texture.EncodeToPNG(), overwrite);
        }

        /// <summary>
        /// Read file from specified path
        /// </summary>
        /// <param name="fullpath">Path to read file</param>
        /// <param name="completed">Callback when the file is read</param>
        public static void ReadFile(string fullpath, Action<AsyncResult<byte[]>> completed)
        {
            ReadFileAsync(fullpath).ContinueWith(result => completed?.Invoke(result.Result), 
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<byte[]>> ReadFileAsync(string fullpath)
        {
            try
            {
                fullpath = FormatPath(fullpath);
                byte[] data = null;
                using (FileStream reader = File.OpenRead(fullpath))
                {
                    data = new byte[reader.Length];
                    await reader.ReadAsync(data, 0, data.Length);
                }

                return new AsyncResult<byte[]>(data, string.Empty);
            }
            catch (Exception e)
            {
                return new AsyncResult<byte[]>(null, e.ToString());
            }
        }

        /// <summary>
        /// Save file at specified path, if overwrite is false a new file will be created
        /// with a number to indicates the new file
        /// </summary>
        /// <param name="fullpath">Path to save file</param>
        /// <param name="data">File content to be saved as a byte array</param>
        /// <param name="overwrite">Overwrite the file or not</param>
        /// <param name="completed">Callback when the file is saved</param>
        public static void WriteFile(string fullpath, byte[] data, bool overwrite, AsyncCallback<bool> completed)
        {
            WriteFileAsync(fullpath, data, overwrite).ContinueWith(result => completed?.Invoke(result.Result), 
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<bool>> WriteFileAsync(string fullpath, byte[] data, bool overwrite)
        {
            try
            {
                fullpath = FormatPath(fullpath);
                CreateDirectoryStructure(fullpath);

                if (!overwrite)
                    fullpath = GetNextFileName(fullpath);

                using (FileStream writer = File.Create(fullpath))
                    await writer.WriteAsync(data, 0, data.Length);

                return new AsyncResult<bool>(true, string.Empty);
            }
            catch (Exception e)
            {
                return new AsyncResult<bool>(false, e.ToString());
            }
        }

        public static void CopyFiles(List<string> filesPath, string destFolder, AsyncCallback<bool> completed, Action<float> progress = null)
        {
            CopyFilesAsync(filesPath, destFolder, progress).ContinueWith(result => completed?.Invoke(result.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<bool>> CopyFilesAsync(List<string> filesPath, string destFolder, Action<float> progress = null)
        {
            List<string> destsPath = filesPath.Select(path =>
            {
                if (File.Exists(path))
                    return FormatPath(destFolder) + "/" + new FileInfo(path).Name;
                else
                    return string.Empty;
            }).Where(path => !string.IsNullOrEmpty(path)).ToList();
            return await CopyFilesAsync(filesPath, destsPath, progress);
        }

        public static void CopyFiles(List<string> filesPath, List<string> destsPath, AsyncCallback<bool> completed, Action<float> progress = null)
        {
            CopyFilesAsync(filesPath, destsPath, progress).ContinueWith(result => completed?.Invoke(result.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<bool>> CopyFilesAsync(List<string> filesPath, List<string> destsPath, Action<float> progress = null)
        {
            if (filesPath.Count < destsPath.Count)
                return new AsyncResult<bool>(false, "Don't have enough destinations path");

            filesPath = filesPath.Where(path => File.Exists(path)).ToList();
            List<Task> tasks = new List<Task>();
            int copiedFiles = 0;
            for (int i = 0; i < filesPath.Count; i++)
            {
                string path = filesPath[i];
                try
                {
                    FileInfo info = new FileInfo(path);
                    tasks.Add(
                        CopyFileAsync(path, FormatPath(destsPath[i]) + "/" + info.Name)
                            .ContinueWith(_ =>
                            {
                                progress?.Invoke(copiedFiles / filesPath.Count);
                                copiedFiles++;
                            })
                    );
                }
                catch { }
            }

            try
            {
                await Task.WhenAll(tasks);
                progress?.Invoke(copiedFiles / filesPath.Count);
            }
            catch (Exception e)
            {
                return new AsyncResult<bool>(false, e.ToString());
            }
            return new AsyncResult<bool>(true, string.Empty);
        }

        /// <summary>
        /// Copy file from source to destination
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destPath">Destination path</param>
        /// <param name="completed">Callback when the file is copied</param>
        public static void CopyFile(string sourcePath, string destPath, AsyncCallback<bool> completed)
        {
            CopyFileAsync(sourcePath, destPath).ContinueWith(result => completed?.Invoke(result.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<bool>> CopyFileAsync(string sourcePath, string destPath)
        {
            try
            {
                using (FileStream sourceStream = File.OpenRead(sourcePath))
                {
                    using (FileStream destStream = File.Create(destPath))
                    {
                        await sourceStream.CopyToAsync(destStream);
                        return new AsyncResult<bool>(true, string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                return new AsyncResult<bool>(false, e.ToString());
            }
        }

        /// <summary>
        /// Copies an entire directory with all folders and subfolders as well as all files
        /// </summary>
        /// <param name="sourceDir">Source directory</param>
        /// <param name="destDir">Destination directory</param>
        /// <param name="completed">Callback when the directory is copied</param>
        /// <param name="progress">Callback to indicate the progress of the copy</param>
        public static void CopyDir(string sourceDir, string destDir, AsyncCallback<bool> completed, Action<float> progress = null)
        {
            CopyDirAsync(sourceDir, destDir, progress).ContinueWith(result => completed?.Invoke(result.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        public static async Task<AsyncResult<bool>> CopyDirAsync(string sourceDir, string destDir, Action<float> progress = null)
        {
            sourceDir = FormatPath(sourceDir);
            destDir = FormatPath(destDir);
            DirectoryInfo source = new DirectoryInfo(sourceDir);
            List<FileInfo> filesToCopy = new List<FileInfo>();

            foreach (var dir in source.GetDirectories("*", SearchOption.AllDirectories))
            {
                try
                {
                    filesToCopy.AddRange(dir.EnumerateFiles().Where(file => !file.FullName.EndsWith(".meta")));
                    string destDirPath = FormatPath(dir.FullName).Replace(sourceDir, $"{destDir}/{source.Name}");
                    if (!Directory.Exists(destDirPath))
                        Directory.CreateDirectory(destDirPath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }

            List<Task> tasks = new List<Task>();
            float copiedFiles = 0;
            foreach (var file in filesToCopy)
            {
                try
                {
                    tasks.Add(
                        CopyFileAsync(file.FullName, FormatPath(file.FullName).Replace(sourceDir, $"{destDir}/{source.Name}"))
                        .ContinueWith(_ =>
                        {
                            progress?.Invoke(copiedFiles / filesToCopy.Count);
                            copiedFiles++;
                        })
                    );
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }
            
            try
            {
                await Task.WhenAll(tasks);
                progress?.Invoke(copiedFiles / filesToCopy.Count);
            }
            catch (Exception e)
            {
                return new AsyncResult<bool>(false, e.ToString());
            }
            return new AsyncResult<bool>(true, string.Empty);
        }

        public static List<string> GetFilesPathAt(string folderPath, SearchOption searchOption, params string[] searchPatterns)
        {
            List<string> paths = new List<string>();
            foreach (var pattern in searchPatterns)
                paths.AddRange(GetFilesPathAt(folderPath, searchOption, pattern));
            return paths;
        }
        public static List<string> GetFilesPathAt(string folderPath, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            List<string> filesPath = new List<string>();
            try
            {
                if (Directory.Exists(folderPath))
                    filesPath = new List<string>(Directory.GetFiles(folderPath, searchPattern, searchOption));
            }
            catch { }
            return filesPath;
        }

        /// <summary>
        /// Check for pendrive connection every 3 seconds
        /// </summary>
        private static void WatchPendriveConnection()
        {
            Playtable.Instance.StartCoroutine(WatchPendriveConnectionRoutine());
        }
        private static IEnumerator WatchPendriveConnectionRoutine()
        {
            while (ConnectedPendrive != null)
            {
                if (!Directory.Exists(ConnectedPendrive.RootDirectory))
                {
                    OnDisconnectedPendrive?.Invoke();
                    ConnectedPendrive = null;
                }
                yield return new WaitForSeconds(3);
            }
        }

        /// <summary>
        /// This will create the directory structure if needed
        /// </summary>
        /// <param name="path">Directory path to be created</param>
        private static void CreateDirectoryStructure(string path)
        {
            string dirPath = path;
            if (path.Contains("."))
                dirPath = new FileInfo(path).DirectoryName;

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }
        /// <summary>
        /// Get the next file name if the current file already exist for example
        /// file abc.png existe, so the next name will be abc (1).png
        /// </summary>
        /// <param name="fullpath">File path to check</param>
        /// <returns>Return the next file name</returns>
        private static string GetNextFileName(string fullpath)
        {
            if (!fullpath.Contains(".")) return fullpath;
            if (!File.Exists(fullpath)) return fullpath;

            string directory = Path.GetDirectoryName(fullpath);
            string fileName = Path.GetFileNameWithoutExtension(fullpath);
            string extension = Path.GetExtension(fullpath);

            int count = 0;
            do
            {
                fullpath = $"{directory}/{fileName} ({++count}){extension}";
            } while (File.Exists(fullpath));

            return FormatPath(fullpath);
        }
        /// <summary>
        /// Format path to keep it standard
        /// </summary>
        /// <param name="path">Path to be formated</param>
        /// <returns>Formated paths</returns>
        private static string FormatPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            path = path.Replace("\\", "/");
            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            return path;
        }
    }
}
