namespace MetX.Web.Virtual.xsl {
   /// <summary>Provides access to static virtual file content for files</summary>
   public partial class editCookie {
       /// <summary>The static contents of the file: "C:\data\code\xlg\MetX\Web\Virtual\xsl\editCookie.xsl" as it existed at compile time.</summary>
       public const string xsl = "<?xml version=\"1.0\" ?>\r\n<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\r\n               \r\n                xmlns:xlg=\"urn:xlg\"\r\n                version=\"1.0\">\r\n  <xsl:output method=\"html\" />\r\n  <xsl:template match=\"/xlgDoc\">\r\n    <HTML>\r\n      <HEAD>\r\n        <title>Security Cookie Editor</title>\r\n        <LINK REL=\"StyleSheet\" TYPE=\"text/css\" HREF=\"/xlgSupport/css/Styles.css\" />\r\n      </HEAD>\r\n      <body>\r\n		    <H1>Security Cookie Editor</H1>\r\n		    <H2>Application: <font color=\"blue\"><xsl:value-of select=\"/xlgDoc/@Application\"/></font></H2>\r\n        <H3><a class=\"buttonLink\"><xsl:attribute name=\"href\">editApplication.aspx?Application=<xsl:value-of select=\"/xlgDoc/@Application\"/></xsl:attribute>Return to Security Application</a></H3>\r\n        <H2>Select the user to set the security cookie to</H2>\r\n        <H3>Current Cookie Value (<xsl:value-of select=\"/xlgDoc/@CookieName\"/>) = '<xsl:value-of select=\"/xlgDoc/@CookieValue\"/>'</H3>\r\n        <table border=\"1\" cellpadding=\"3\" cellspacing=\"1\">\r\n           <tr class=\"subHeaderContent\">\r\n            <td width=\"100\">&amp;nbsp;</td>\r\n             <td>Set cookie to this user</td>\r\n             <td>User Name</td>\r\n             <td>Full Name</td>\r\n             <td>Category</td>\r\n           </tr>\r\n        <xsl:if test=\"/xlgDoc/@Application='%%YOURAPPNAME%%'\">\r\n           <tr>\r\n            <td></td>\r\n             <td><a class=\"buttonLink\" href=\"editCookie.aspx?Application=%%YOURAPPNAME%%&amp;UserName=%%YOURUSERNAME%%&amp;PostAction=setcookie\">Set</a></td>\r\n             <td>%%YOURUSERNAME%%</td>\r\n             <td>William M. Rawls</td>\r\n             <td></td>\r\n           </tr>\r\n           <tr>\r\n            <td></td>\r\n             <td><a class=\"buttonLink\" href=\"editCookie.aspx?Application=%%YOURAPPNAME%%&amp;UserName=%%YOURUSERNAME%%&amp;PostAction=setcookie\">Set</a></td>\r\n             <td>%%YOURUSERNAME%%</td>\r\n             <td>Rob Reisinger</td>\r\n             <td></td>\r\n           </tr>\r\n           <tr>\r\n            <td></td>\r\n             <td><a class=\"buttonLink\" href=\"editCookie.aspx?Application=%%YOURAPPNAME%%&amp;UserName=admin&amp;PostAction=setcookie\">Set</a></td>\r\n             <td>admin</td>\r\n             <td>AcademyX admin</td>\r\n             <td></td>\r\n           </tr>\r\n           <tr>\r\n            <td></td>\r\n             <td><a class=\"buttonLink\" href=\"editCookie.aspx?Application=%%YOURAPPNAME%%&amp;UserName=10005&amp;PostAction=setcookie\">Set</a></td>\r\n             <td>10005</td>\r\n             <td>Jeff Allen</td>\r\n             <td></td>\r\n           </tr>\r\n        </xsl:if>\r\n        <xsl:for-each select=\"/xlgDoc/Members/User\">\r\n          <tr>\r\n						<xsl:choose>\r\n							<xsl:when test=\"position() mod 2 = 0\">\r\n								<xsl:attribute name=\"class\">contentDataRow1</xsl:attribute>\r\n							</xsl:when>\r\n							<xsl:otherwise>\r\n								<xsl:attribute name=\"class\">contentDataRow2</xsl:attribute>\r\n							</xsl:otherwise>\r\n						</xsl:choose>\r\n            <td></td>\r\n            <td>\r\n              <a class=\"buttonLink\"><xsl:attribute name=\"href\">editCookie.aspx?UserName=<xsl:value-of select=\"@UserName\" />&amp;PostAction=setcookie</xsl:attribute>Set</a>\r\n            </td>\r\n            <td nowrap=\"nowrap\"><xsl:value-of select=\"@UserName\" /></td>\r\n            <td nowrap=\"nowrap\"><xsl:value-of select=\"@FullName\" /></td>\r\n            <td nowrap=\"nowrap\"><xsl:value-of select=\"@Category\" /></td>\r\n          </tr>\r\n        </xsl:for-each>\r\n        </table>\r\n      </body>\r\n    </HTML>\r\n  </xsl:template>\r\n</xsl:stylesheet>";
       /// <summary>Returns xsl inside a StringBuilder.</summary>
       /// <returns>A StringBuilder with the compile time file contents</returns>
       public static System.Text.StringBuilder xslStringBuilder { get { return new System.Text.StringBuilder(xsl); } }
   }
}
