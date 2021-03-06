namespace CommonFunctions.WebApiStructs.Request
{
    public class ReqMoidfyCameraInstance
    {
        private string _cameraId;
        private string? _cameraName;
        private string? _deptId;
        private string? _deptName;
        private string? _pDeptId;
        private string? _pDeptName;
        private bool? _enableLive;
        private bool? _enablePtz;
        private bool? _enableRecord;
        private bool? _ifGB28181Tcp;

        public string CameraId
        {
            get => _cameraId;
            set => _cameraId = value;
        }

        public string? CameraName
        {
            get => _cameraName;
            set => _cameraName = value;
        }

        public string? DeptId
        {
            get => _deptId;
            set => _deptId = value;
        }

        public string? DeptName
        {
            get => _deptName;
            set => _deptName = value;
        }

        public string? PDeptId
        {
            get => _pDeptId;
            set => _pDeptId = value;
        }

        public string? PDeptName
        {
            get => _pDeptName;
            set => _pDeptName = value;
        }


        public bool? EnableLive
        {
            get => _enableLive;
            set => _enableLive = value;
        }

        public bool? EnablePtz
        {
            get => _enablePtz;
            set => _enablePtz = value;
        }

        public bool? EnableRecord
        {
            get => _enableRecord;
            set => _enableRecord = value;
        }

        public bool? IfGb28181Tcp
        {
            get => _ifGB28181Tcp;
            set => _ifGB28181Tcp = value;
        }
    }
}