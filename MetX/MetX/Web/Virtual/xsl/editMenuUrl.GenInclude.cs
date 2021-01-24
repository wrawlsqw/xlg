namespace MetX.Web.Virtual.xsl {
   /// <summary>Provides access to static virtual file content for files</summary>
   public partial class EditMenuUrl {
       /// <summary>The static contents of the file: "C:\data\code\wmr\MetX\Web\Virtual\xsl\editMenuUrl.xsl" as it existed at compile time.</summary>
       public const string Xsl = "<?xml version=\"1.0\" ?>\r\n<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:xlg=\"urn:xlg\" version=\"1.0\">\r\n	<xsl:output method=\"html\" omit-xml-declaration=\"yes\" indent=\"yes\" />\r\n	<xsl:template match=\"/\">\r\n    <HTML>\r\n      <HEAD>\r\n        <!--<META HTTP-EQUIV=\"EXPIRES\" CONTENT=\"2/5/99 11:30PM\" />-->\r\n        <LINK REL=\"StyleSheet\" TYPE=\"text/css\"><xsl:attribute name=\"href\"><xsl:value-of select=\"/xlgDoc/@ServerPath\"/>xlgSupport/css/Styles.css</xsl:attribute></LINK>\r\n        <link rel=\"shortcut icon\" href=\"/xlgSupport/images/favicon.ico\" />\r\n        <script langage=\"JavaScript\">\r\n          var menuEditMode = 1;\r\n        </script>\r\n        <SCRIPT language=\"JavaScript\" src=\"/xlgSupport/scripts/dhtmlgoodies_menu.js\" type=\"text/javascript\"></SCRIPT>\r\n        <SCRIPT language=\"JavaScript\" src=\"/xlgSupport/scripts/prototype.js\" type=\"text/javascript\"></SCRIPT>\r\n        <SCRIPT language=\"JavaScript\" src=\"/xlgSupport/scripts/prototype-extensions.js\" type=\"text/javascript\"></SCRIPT>\r\n        <SCRIPT language=\"JavaScript\">\r\n          function initDhtmlGoodiesMenuContinued()\r\n          {\r\n            // $('docarea').src='<xsl:value-of select=\"xlg:sReplace(xlg:sReplace(/xlgDoc/Mindset/@StartPage, '[UserName]', /xlgDoc/SecureUserPage/@UserName),'[SecurityToken]',/xlgDoc/@SecurityToken)\"/>';\r\n          }\r\n        </SCRIPT>\r\n      </HEAD>\r\n      <BODY LEFTMARGIN=\"0\" RIGHTMARGIN=\"0\" TOPMARGIN=\"0\" marginheight=\"0\" marginwidth=\"0\" SCROLL=\"NO\" class=\"container\">\r\n        <div id=\"mainContainer\">\r\n          <div id=\"dhtmlgoodies_menu\">\r\n            <ul>\r\n              <li><a title=\"Click to refresh entire page (including menu)\"><xsl:attribute name=\"href\"><xsl:value-of select=\"/xlgDoc/@ProbableURL\"/></xsl:attribute><img src=\"/xlgSupport/images/star.bmp\" /></a></li>\r\n              <xsl:apply-templates select=\"/xlgDoc/Mindset/MindsetMenu\" mode=\"MainMenu\" />\r\n              <xsl:apply-templates select=\"/xlgDoc/Mindset/MindsetMenu\" mode=\"MainSubmenus\" />\r\n            </ul>\r\n          </div>\r\n        </div>\r\n        <div id=\"docareaDivEditor\">\r\n        <IFRAME ID=\"docarea\" NAME=\"docarea\" marginwidth=\"0\" marginheight=\"0\" FRAMEBORDER=\"1\" SCROLLING=\"YES\">\r\n          Hello <xsl:value-of select=\"/xlgDoc/SecureUserPage/@FullName\"/> (Loading...)\r\n        </IFRAME>\r\n        </div>\r\n      </BODY>\r\n    </HTML>\r\n	</xsl:template>\r\n	\r\n	<xsl:template match=\"MindsetMenu\" mode=\"MainMenu\">\r\n		<xsl:variable name=\"MenuID\" select=\"@MenuID\" />\r\n		<xsl:apply-templates select=\"/xlgDoc/Menus/Menu[@MenuID=$MenuID and @Title='Main']/MenuItem\" mode=\"MainSubmenus\" />\r\n	</xsl:template>\r\n	\r\n	<xsl:template match=\"MindsetMenu\" mode=\"MainSubmenus\">\r\n		<xsl:variable name=\"MenuID\" select=\"@MenuID\" />\r\n		<xsl:if test=\"count(/xlgDoc/Menus/Menu[@MenuID=$MenuID and @Title!='Main' and ((@Requires='1') or (@Requires='2' and /xlgDoc/SecureUserPage/@Update='True') or (@Requires='4' and /xlgDoc/SecureUserPage/@Execute='True') or (@Requires='8' and /xlgDoc/SecureUserPage/@Special='True'))]) &gt; 0\">\r\n			<ul>\r\n				<xsl:apply-templates select=\"/xlgDoc/Menus/Menu[@MenuID=$MenuID and @Title!='Main']\" mode=\"MainSubmenus\" />\r\n			</ul>\r\n		</xsl:if>\r\n	</xsl:template>\r\n	\r\n	<xsl:template match=\"Menu\" mode=\"MainSubmenus\">\r\n		<xsl:variable name=\"MenuID\" select=\"@MenuID\" />\r\n		<xsl:if test=\"(@Requires='1') or (@Requires='2' and /xlgDoc/SecureUserPage/@Update='True') or (@Requires='4' and /xlgDoc/SecureUserPage/@Execute='True') or (@Requires='8' and /xlgDoc/SecureUserPage/@Special='True')\">\r\n			<xsl:choose>\r\n				<xsl:when test=\"count(/xlgDoc/Menus/Menu[@MenuID=$MenuID]/MenuItem) &gt; 0\">\r\n					<li>\r\n						<a target=\"docarea\">\r\n							<xsl:attribute name=\"title\"><xsl:value-of select=\"@Description\" /></xsl:attribute>\r\n							<xsl:value-of select=\"@Title\" />\r\n						</a>\r\n						<ul>\r\n						<xsl:apply-templates select=\"/xlgDoc/Menus/Menu[@MenuID=$MenuID]/MenuItem\" mode=\"MainSubmenus\" />\r\n						</ul>\r\n					</li>\r\n				</xsl:when>\r\n				<xsl:otherwise>\r\n					<xsl:variable name=\"ItemID\" select=\"/xlgDoc/Menus/Menu[@MenuID=$MenuID]/MenuItem/@ItemID\" />\r\n					<xsl:apply-templates select=\"/xlgDoc/Urls/Url[@UrlID=$ItemID]\" />\r\n				</xsl:otherwise>\r\n			</xsl:choose>\r\n		</xsl:if>\r\n	</xsl:template>\r\n	\r\n	<xsl:template match=\"MenuItem\" mode=\"MainSubmenus\">\r\n		<xsl:variable name=\"ItemID\" select=\"@ItemID\" />\r\n		<xsl:choose>\r\n			<xsl:when test=\"@IsSubMenu='1'\">\r\n						<xsl:apply-templates select=\"/xlgDoc/Menus/Menu[@MenuID=$ItemID]\" mode=\"MainSubmenus\" />\r\n			</xsl:when>\r\n			<xsl:otherwise>\r\n				<xsl:apply-templates select=\"/xlgDoc/Urls/Url[@UrlID=$ItemID]\">\r\n          <xsl:with-param name=\"MenuItemID\" select=\"$ItemID\" />\r\n        </xsl:apply-templates>\r\n			</xsl:otherwise>\r\n		</xsl:choose>\r\n	</xsl:template>\r\n\r\n	<xsl:template match=\"Url\">\r\n    <xsl:param name=\"MenuItemID\" />\r\n		<xsl:if test=\"(@Requires='1') or (@Requires='2' and /xlgDoc/SecureUserPage/@Update='True') or (@Requires='4' and /xlgDoc/SecureUserPage/@Execute='True') or (@Requires='8' and /xlgDoc/SecureUserPage/@Special='True')\">\r\n			<li>\r\n				<a target=\"docarea\">\r\n          <xsl:attribute name=\"title\"><xsl:value-of select=\"@Description\" /></xsl:attribute>\r\n          <xsl:attribute name=\"href\">editMenuUrl.aspx?MenuItemID=<xsl:value-of select=\"$MenuItemID\" />&amp;UrlID=<xsl:value-of select=\"@UrlID\"/></xsl:attribute>\r\n					<xsl:value-of select=\"@Title\" />\r\n				</a>\r\n			</li>\r\n		</xsl:if>\r\n	</xsl:template>\r\n</xsl:stylesheet>";
       /// <summary>Returns xsl inside a StringBuilder.</summary>
       /// <returns>A StringBuilder with the compile time file contents</returns>
       public static System.Text.StringBuilder XslStringBuilder { get { return new System.Text.StringBuilder(Xsl); } }
   }
}
