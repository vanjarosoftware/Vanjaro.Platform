using System.Collections.Generic;
using System.Linq;

namespace Vanjaro.Common.Components
{
    public class EditorOptions
    {
        public bool DocumentSource = true;
        public bool DocumentPreview = true;
        public bool DocumentPrint = true;
        public bool DocumentTemplates = true;
        public bool ClipboardCut = true;
        public bool ClipboardCopy = true;
        public bool ClipboardPaste = true;
        public bool ClipboardPasteText = true;
        public bool ClipboardPasteFromWord = true;
        public bool ClipboardUndo = true;
        public bool ClipboardRedo = true;
        public bool EditingFind = true;
        public bool EditingReplace = true;
        public bool EditingSelectAll = true;
        public bool EditingScayt = true;
        public bool BasicStylesBold = true;
        public bool BasicStylesItalic = true;
        public bool BasicStylesUnderline = true;
        public bool BasicStylesStrike = true;
        public bool BasicStylesSubscript = true;
        public bool BasicStylesSuperscript = true;
        public bool BasicStylesCopyFormatting = true;
        public bool BasicStylesRemoveFormat = true;
        public bool ParagraphNumberedList = true;
        public bool ParagraphBulletedList = true;
        public bool ParagraphOutdent = true;
        public bool ParagraphIndent = true;
        public bool ParagraphBlockquote = true;
        public bool ParagraphCreateDiv = true;
        public bool ParagraphJustifyLeft = true;
        public bool ParagraphJustifyCenter = true;
        public bool ParagraphJustifyRight = true;
        public bool ParagraphJustifyBlock = true;
        public bool ParagraphBidiLtr = true;
        public bool ParagraphBidiRtl = true;
        public bool LinksLink = true;
        public bool LinksUnlink = true;
        public bool LinksAnchor = true;
        public bool InsertImage = true;
        public bool InsertFlash = true;
        public bool InsertTable = true;
        public bool InsertHorizontalRule = true;
        public bool InsertSmiley = true;
        public bool InsertSpecialChar = true;
        public bool InsertPageBreak = true;
        public bool InsertIframe = true;
        public bool StylesStyles = true;
        public bool StylesFormat = true;
        public bool StylesFont = true;
        public bool StylesFontSize = true;
        public bool ToolsMaximize = true;
        public bool ToolsShowBlocks = true;
        public bool ColorsTextColor = true;
        public bool ColorsBGColor = true;
        public bool AboutAbout = true;
        public string UiColor = "#FAFAFA";
        public string Height = "300px";
        public string Width = "100%";
        public bool FilebrowserBrowseUrl = true;
        public bool FilebrowserImageBrowseUrl = true;
        public Dictionary<string, bool> Plugins { get; set; }
        public List<string> BasicPlugins => "autolink,basicstyles,blockquote,clipboard,toolbar,enterkey,entities,floatingspace,wysiwygarea,indentlist,link,list,undo,dialog,dialogui,fakeobjects,indent,notification,button,magicline,image,filebrowser".Split(',').ToList();
        public List<string> MinimalPlugins => "autolink,basicstyles,blockquote,clipboard,toolbar,enterkey,entities,floatingspace,wysiwygarea,link,list,undo,dialog,dialogui,fakeobjects,notification,button,magicline".Split(',').ToList();
        public List<string> StandardPlugins => "a11yhelp,autolink,basicstyles,blockquote,clipboard,contextmenu,resize,toolbar,elementspath,enterkey,entities,filebrowser,floatingspace,format,horizontalrule,htmlwriter,wysiwygarea,image,indentlist,link,list,magicline,maximize,pastetext,pastefromword,removeformat,showborders,sourcearea,specialchar,scayt,stylescombo,tab,table,tableselection,tabletools,undo,uploadimage,wsc,dialog,dialogui,fakeobjects,filetools,floatpanel,indent,lineutils,listblock,menu,menubutton,notification,notificationaggregator,panel,popup,richcombo,button,uploadwidget,widget,widgetselection".Split(',').ToList();
        public List<string> FullPlugins => "a11yhelp,autolink,dialogadvtab,basicstyles,bidi,blockquote,clipboard,colorbutton,colordialog,templates,contextmenu,copyformatting,div,resize,toolbar,elementspath,enterkey,entities,filebrowser,find,flash,floatingspace,font,forms,format,horizontalrule,htmlwriter,iframe,wysiwygarea,image,indentblock,indentlist,smiley,justify,language,link,list,liststyle,magicline,maximize,newpage,pagebreak,pastetext,pastefromword,preview,print,removeformat,save,selectall,showblocks,showborders,sourcearea,specialchar,scayt,stylescombo,tab,table,tableselection,tabletools,undo,uploadimage,wsc,dialog,dialogui,fakeobjects,filetools,floatpanel,indent,lineutils,listblock,menu,menubutton,notification,notificationaggregator,panel,panelbutton,popup,richcombo,button,uploadwidget,widget,widgetselection".Split(',').ToList();
    }
}
