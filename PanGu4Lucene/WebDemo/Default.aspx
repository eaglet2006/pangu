<%@ Register Assembly="AspDotNetPager" Namespace="Eaglet.Workroom.AspDotNetPager"
    TagPrefix="cc1" %>

<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>盘古分词 Web 演示</title>
    
<style type="text/css"><!--
body,td,.p1,.p2,.i{font-family:arial}
body{margin:6px 0 0 0;background-color:#fff;color:#000;}
table{border:0}
TD{FONT-SIZE:9pt;LINE-HEIGHT:18px;}
.f14{FONT-SIZE:14px}
.f10{font-size:10.5pt}
.f16{font-size:16px;font-family:Arial}
.c{color:#7777CC;}
.p1{LINE-HEIGHT:120%;margin-left:-12pt}
.p2{width:100%;LINE-HEIGHT:120%;margin-left:-12pt}
.i{font-size:16px}
.t{COLOR:#0000cc;TEXT-DECORATION:none}
a.t:hover{TEXT-DECORATION:underline}
.p{padding-left:18px;font-size:14px;word-spacing:4px;}
.f{line-height:120%;font-size:100%;width:32em;padding-left:15px;word-break:break-all;word-wrap:break-word;}
.h{margin-left:8px;width:100%}
.s{width:8%;padding-left:10px; height:25px;}
.m,a.m:link{COLOR:#666666;font-size:100%;}
a.m:visited{COLOR:#660066;}
.g{color:#008000; font-size:12px;}
.r{ word-break:break-all;cursor:hand;width:225px;}
.bi {background-color:#D9E1F7;height:20px;margin-bottom:12px}
.pl{padding-left:3px;height:8px;padding-right:2px;font-size:14px;}
.Tit{height:21px; font-size:14px;}
.fB{ font-weight:bold;}
.mo,a.mo:link,a.mo:visited{COLOR:#666666;font-size:100%;line-height:10px;}
.htb{margin-bottom:5px;}
#ft{clear:both;line-height:20px;background:#E6E6E6;text-align:center}
#ft,#ft *{color:#77C;font-size:12px;font-family:Arial}
#ft span{color:#666}
.align-center{   
    margin:0 auto;      /* 居中 这个是必须的，，其它的属性非必须 */   
    text-align:center;  /* 文字等内容居中 */}
       
--></style>
    
</head>
<body>
    <p>
        <br />
    </p>
    <form id="form1" runat="server">
    <div class = "align-center">
    <a href="http://pangusegment.codeplex.com/Wiki/View.aspx?title=Home">
        <asp:Image ID="Image1" runat="server" Width = "197px" Height = "100px"
            ImageUrl="http://i3.codeplex.com/Project/Download/FileDownload.aspx?ProjectName=pangusegment&amp;DownloadId=79006" 
            /></a>
        <br />
        <br />
        <asp:TextBox ID="TextBoxSearch" runat="server" ></asp:TextBox>&nbsp;
        <asp:Button ID="ButtonSearch" runat="server" Text="Search" OnClick="ButtonSearch_Click" />&nbsp;
        <asp:Label ID="LabelDuration" runat="server" Text="用时:"></asp:Label>
        <br />
            </div>

        <div>
        <br />
        <asp:Table ID="TableList" runat="server">
        </asp:Table>
        <cc1:AspNetPager id="AspNetPager" runat="server" OnPageChanged="AspNetPager_PageChanged" PageSize="10">
        </cc1:AspNetPager>
        </div>
    </form>

</body>
</html>
