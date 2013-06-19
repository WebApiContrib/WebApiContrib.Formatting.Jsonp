<%@Page Language="C#" %>
<%
	var webClient = new System.Net.WebClient();
	var triggersStackOverflowException = webClient.DownloadString("http://localhost:53425/api/value/1");
	Response.Write(triggersStackOverflowException);
%>
