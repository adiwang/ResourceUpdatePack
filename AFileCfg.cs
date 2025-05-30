using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

public enum AFileCompressionMethod
{
	None = 0,	//不压缩
	Zlib,
	Lz4,
}

public class AFileCfg
{
	public static AFileCfg LoadFromXmlFile(String xmlPath)
	{
		AFileCfg obj = new AFileCfg();
		obj.InitFromFileXmlFile(xmlPath);
		return obj;
	}

	private AFileCfg()
	{
	}

	private void InitFromFileXmlFile(String xmlPath)
	{
		XDocument xdoc = XDocument.Load(xmlPath);
		XElement rootElement = xdoc.Root;
		{
			XElement packagesElement = rootElement.Element("packages");
			List<String> packages = new List<String>();
			foreach (XElement packageElement in packagesElement.Elements("package"))
			{
				packages.Add(packageElement.Attribute("name").Value.ToLower());
			}
			m_packageList = packages.ToArray();
			m_packageSet = new HashSet<String>();
			foreach (String package in m_packageList)
				m_packageSet.Add(package);
		}
		{
			XElement compressionElement = rootElement.Element("compression");
			String defaultMethod = compressionElement.Attribute("default_method").Value;
			m_defaultCompressionMethod = CompressionMethodNameToValue(defaultMethod);
			m_fileExtToCompressionMethod = new Dictionary<String, AFileCompressionMethod>();
			foreach (XElement fileTypeElement in compressionElement.Elements("file_type"))
			{
				String ext = fileTypeElement.Attribute("ext").Value.ToLower();
				String method = fileTypeElement.Attribute("method").Value;
				m_fileExtToCompressionMethod.Add(ext, CompressionMethodNameToValue(method));
			}
		}
	}

	private AFileCompressionMethod CompressionMethodNameToValue(String methodName)
	{
		switch (methodName)
		{
			case "none":
				return AFileCompressionMethod.None;
			case "zlib":
				return AFileCompressionMethod.Zlib;
			case "lz4":
				return AFileCompressionMethod.Lz4;
			default:
				throw new Exception("invalid compress method name: " + methodName);
		}
	}

	public String[] PackageList
	{
		get { return m_packageList; }
	}

	public Boolean PackageListContains(String packageName)
	{
		return m_packageSet.Contains(packageName);
	}

	public String UnifyPath(String path)
	{
		return path.ToLowerInvariant().Replace('\\', '/');
	}

	public Boolean PackageListContainsForPath(String path)
	{
		String unifiedPath = UnifyPath(path);
		String packageName;
		{
			Int32 iFirstSep = unifiedPath.IndexOf('/');
			if (iFirstSep >= 0)
				packageName = unifiedPath.Substring(0, iFirstSep);
			else
				packageName = unifiedPath;
		}
		return PackageListContains(packageName);
	}

	public AFileCompressionMethod DefaultCompressionMethod
	{
		get { return m_defaultCompressionMethod; }
	}

	public AFileCompressionMethod GetCompressionMethod(String extension)
	{
		AFileCompressionMethod method;
		if (m_fileExtToCompressionMethod.TryGetValue(extension, out method))
			return method;
		else
			return m_defaultCompressionMethod;
	}

	public AFileCompressionMethod GetCompressionMethodForPath(String path)
	{
		return GetCompressionMethod(Path.GetExtension(path).ToLower());
	}

	private String[] m_packageList;
	private HashSet<String> m_packageSet;
	private AFileCompressionMethod m_defaultCompressionMethod;
	private Dictionary<String, AFileCompressionMethod> m_fileExtToCompressionMethod;
}
