namespace Vanjaro.Common.Components
{
    public class HTMLEditorSetting
    {
        public bool UploadFiles = false;
        public string UploadFilesAllowedAttachmentFileExtensions = "jpg,jpeg,gif,png,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,zip,rar";
        public int UploadFilesMaxFileSize = 10;
        public int UploadFilesRootFolder = 0;
        public bool UploadImages = false;
        public string UploadImagesAllowedAttachmentFileExtensions = "jpg,jpeg,gif,png";
        public int UploadImagesMaxFileSize = 5;
        public int UploadImagesRootFolder = 0;
        public bool FullPageMode = false;
    }
}