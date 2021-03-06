using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace StreamMediaServerKeeper
{
    public class ProcessApis
    {
        private uint _pid;

        private uint _isRunning
        {
            get
            {
                if (checkProcessExists())
                {
                    return _pid;
                }

                return 0;
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public bool FileExists(string filePath, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            return File.Exists(filePath);
        }

        /// <summary>
        /// 删除一个文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public bool DeleteFile(string filePath, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return true;
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="filePathList"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public bool DeleteFileList(List<string> filePathList, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (filePathList != null && filePathList.Count > 0)
            {
                foreach (var filePath in filePathList)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 删除录制目录中的空目录
        /// </summary>
        public bool ClearNoFileDir(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            try
            {
                string dvrPath = Common.RecordPath;
                if (Directory.Exists(dvrPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(dvrPath);
                    DirectoryInfo[] subdirs = dir.GetDirectories("*.*", SearchOption.AllDirectories);
                    foreach (DirectoryInfo subdir in subdirs)
                    {
                        FileSystemInfo[] subFiles = subdir.GetFileSystemInfos();
                        if (subFiles.Length == 0)
                        {
                            subdir.Delete();
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查流媒体是否正在运行
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public uint CheckIsRunning(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            return _isRunning;
        }

        /// <summary>
        /// 关闭流媒体
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public bool StopServer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            int i = 0;
            while (checkProcessExists() && i < 10)
            {
                string cmd = "";
                if (i < 5)
                {
                    cmd = "kill -2 " + _pid.ToString();
                }
                else
                {
                    cmd = "kill -9 " + _pid.ToString();
                }

                LinuxShell.Run(cmd, 500);
                Thread.Sleep(200);
                i++;
            }

            if (checkProcessExists())
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Other,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Other],
                };
                return false;
            }

            return true;
        }

        /// <summary>
        /// 重启流媒体服务
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public uint RestartServer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (checkProcessExists())
            {
                StopServer(out rs);
            }

            Thread.Sleep(2000);
            return RunServer(out rs);
        }

        /// <summary>
        /// 启动流媒体服务
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public uint RunServer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (File.Exists(Common.MediaServerBinPath))
            {
                if (!checkProcessExists()) //如果不存在，就执行
                {
                    string dir = Path.GetDirectoryName(Common.MediaServerBinPath) + "/";
                    if (!Directory.Exists(dir + "log"))
                    {
                        Directory.CreateDirectory(dir + "log");
                    }

                    string stdout = "";
                    string errout = "";
                    string cmd = "ulimit -c unlimited";
                    LinuxShell.Run(cmd, 500); //执行取消限制
                    //  cmd = Common.MediaServerBinPath + " -d &";
                    cmd = "nohup " + Common.MediaServerBinPath + " > " + dir + "log/MServerRun.log &";
                    LinuxShell.Run(cmd, 1000);
                    int i = 0;
                    while (!checkProcessExists() && i < 50)
                    {
                        i++;
                        Thread.Sleep(100);
                    }

                    if (checkProcessExists())
                    {
                        return _pid;
                    }

                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.ZLMediaKitRunBinExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.ZLMediaKitRunBinExcept] + "\r\n" + stdout +
                                  "\r\n" + errout,
                    };
                    return 0;
                }

                return _pid; //已经启动着的，不用重复启动
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.ZLMediaKitBinNotFound,
                Message = ErrorMessage.ErrorDic![ErrorNumber.ZLMediaKitBinNotFound],
            };
            return 0;
        }

        /// <summary>
        /// 用可执行文件路径确定进程是否正在运行
        /// </summary>
        /// <returns></returns>
        private bool checkProcessExists()
        {
            string cmd = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                cmd = "ps -aux |grep " + Common.MediaServerBinPath + "|grep -v grep|awk \'{print $2}\'";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                cmd = "ps -A |grep " + Common.MediaServerBinPath + "|grep -v grep|awk \'{print $1}\'";
            }

            string stdout = "";
            string errout = "";
            var ret = LinuxShell.Run(cmd, 300, out stdout, out errout);
            if (!string.IsNullOrEmpty(stdout) && ret)
            {
                if (uint.TryParse(stdout, out _pid))
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(errout) && ret)
            {
                if (uint.TryParse(errout, out _pid))
                {
                    return true;
                }
            }

            return false;
        }
    }
}