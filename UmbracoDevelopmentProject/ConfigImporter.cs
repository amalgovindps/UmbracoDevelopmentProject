using System.ComponentModel;
using System.Drawing;
using System.Xml;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace UmbracoDevelopmentProject
{
	public class ConfigImporter
	{
		private readonly IContentTypeService _contentService;
		private readonly IShortStringHelper _shortStringHelper;
		private readonly IDataTypeService _dataTypeService;
		public ConfigImporter(IContentTypeService contentService, IShortStringHelper shortStringHelper, IDataTypeService dataTypeService)
		{
			_contentService = contentService;
			_shortStringHelper = shortStringHelper;
			_dataTypeService = dataTypeService;
		}

		public void ImportXml(string xmlFilePath)
		{
			try
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(xmlFilePath);

				foreach (XmlNode node in xmlDoc.SelectNodes("/ContentType"))
				{
					Guid ContentTypeKey = Guid.Parse(node.Attributes["Key"].Value);
					string ContentTypeAlias = node.Attributes["Alias"].Value;
					int ContentTypeLevel = int.Parse(node.Attributes["Level"].Value);

					XmlNode infoNode = node.SelectSingleNode("Info");
					string Name = infoNode.SelectSingleNode("Name").InnerText;
					string Icon = infoNode.SelectSingleNode("Icon").InnerText;
					string Thumbnail = infoNode.SelectSingleNode("Thumbnail").InnerText;
					string Description = infoNode.SelectSingleNode("Description").InnerText;
					bool AllowAtRoot = bool.Parse(infoNode.SelectSingleNode("AllowAtRoot").InnerText);
					string IsListView = infoNode.SelectSingleNode("IsListView").InnerText;
					string variationsValue = infoNode.SelectSingleNode("Variations").InnerText;
					int Variations = int.TryParse(variationsValue, out int variations) ? variations : 0;
					bool IsElement = bool.Parse(infoNode.SelectSingleNode("IsElement").InnerText);
					string Folder = infoNode.SelectSingleNode("Folder").InnerText;

					IContentType contentType = new ContentType(_shortStringHelper, -1)
					{
						Alias = ContentTypeAlias,
						Key = ContentTypeKey,
						Level = ContentTypeLevel,
						Name = Name,
						Icon = Icon,
						Thumbnail = Thumbnail,
						Description = Description,
						AllowedAsRoot = AllowAtRoot,
						IsContainer = IsListView == "List",
						Variations = (ContentVariation)Variations,
						IsElement = IsElement


					};
					// Extract and create tabs as groups
					XmlNodeList tabNodes = node.SelectNodes("Tabs/Tab");
					foreach (XmlNode tabNode in tabNodes)
					{
						// Extract tab information
						Guid tabKey = Guid.Parse(tabNode.SelectSingleNode("Key").InnerText);
						string tabCaption = tabNode.SelectSingleNode("Caption").InnerText;
						string tabAlias = tabNode.SelectSingleNode("Alias").InnerText;
						string tabType = tabNode.SelectSingleNode("Type").InnerText;

						// Create tab as a group
						PropertyGroup tabGroup = new PropertyGroup(true)
						{
							Name = tabCaption,
							Key = tabKey,
							Alias = tabAlias


						};

						// Add tab group to ContentType
						contentType.PropertyGroups.Add(tabGroup);

						// Extract and create properties within the tab
						XmlNodeList propertyNodes = node.SelectNodes("GenericProperties/GenericProperty");
						foreach (XmlNode propertyNode in propertyNodes)
						{
							// Extract property information
							Guid propertyKey = Guid.Parse(propertyNode.SelectSingleNode("Key").InnerText);
							string propertyName = propertyNode.SelectSingleNode("Name").InnerText;
							string propertyAlias = propertyNode.SelectSingleNode("Alias").InnerText;
							string propertyDefinition = propertyNode.SelectSingleNode("Definition").InnerText;
							string propertyType = propertyNode.SelectSingleNode("Type").InnerText;
							bool propertyMandatory = bool.Parse(propertyNode.SelectSingleNode("Mandatory").InnerText);
							string propertyValidation = propertyNode.SelectSingleNode("Validation").InnerText;
							string propertyDescription = propertyNode.SelectSingleNode("Description").InnerText;
							int propertySortOrder = int.Parse(propertyNode.SelectSingleNode("SortOrder").InnerText);
							string propertyTab = propertyNode.SelectSingleNode("Tab").InnerText;
							string propertyVariation = propertyNode.SelectSingleNode("Variations").InnerText;
							int propertyVariations = 0;
							if (propertyVariation == "Nothing")
							{
								propertyVariations = 0;
							}
							else
							{
								propertyVariations = int.Parse(propertyVariation);
							}
							string propertyMandatoryMessage = propertyNode.SelectSingleNode("MandatoryMessage").InnerText;
							string propertyValidationRegExpMessage = propertyNode.SelectSingleNode("ValidationRegExpMessage").InnerText;
							bool propertyLabelOnTop = bool.Parse(propertyNode.SelectSingleNode("LabelOnTop").InnerText);

							IDataType dataType = _dataTypeService.GetByEditorAlias(propertyType)?.FirstOrDefault();

							if (dataType != null)
							{
								PropertyType property = new PropertyType(_shortStringHelper, dataType)
								{
									Name = propertyName,
									Key = propertyKey,
									Alias = propertyAlias,
									Description = propertyDescription,
									Mandatory = propertyMandatory,
									Variations = (ContentVariation)propertyVariations,
									SortOrder = propertySortOrder,
									ValidationRegExp = propertyValidation,
									MandatoryMessage = propertyMandatoryMessage,
									ValidationRegExpMessage = propertyValidationRegExpMessage,
									LabelOnTop = propertyLabelOnTop
								};

								// Add property to tab group
								tabGroup.PropertyTypes.Add(property);
							}
							else
							{
								// Handle unsupported data type or throw an exception
								throw new Exception($"Unsupported data type: {propertyType}");
							}
						}

						// Save ContentType
						_contentService.Save(contentType);
					}
				}

				Console.WriteLine("XML import successful!");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error importing XML: {ex.Message}");
			}
		}

		// Method to retrieve or create the corresponding IDataType instance based on the property type alias

	}
}





















//using System;
//using System.Collections.Generic;
//using System.Xml;
//using Umbraco.Cms.Core.Models;
//using Umbraco.Cms.Core.Models.ContentEditing;
//using Umbraco.Cms.Core.Services;
//using Umbraco.Cms.Core.Strings;

//namespace UmbracoDevelopmentProject
//{
//	public class ConfigImporter
//	{
//		private readonly IContentTypeBaseService<IContentTypeComposition> _contentService;
//		private readonly IShortStringHelper _shortStringHelper;

//		public ConfigImporter(IContentTypeBaseService<IContentTypeComposition> contentService, IShortStringHelper shortStringHelper)
//		{
//			_contentService = contentService;
//			_shortStringHelper = shortStringHelper;
//		}

//		public void ImportXml(string xmlFilePath)
//		{
//			try
//			{
//				XmlDocument xmlDoc = new XmlDocument();
//				xmlDoc.Load(xmlFilePath);

//				foreach (XmlNode node in xmlDoc.SelectNodes("/ContentType"))
//				{
//					Guid contentTypeKey = Guid.Parse(node.Attributes["Key"].Value);
//					string contentTypeAlias = node.Attributes["Alias"].Value;
//					int contentTypeLevel = int.Parse(node.Attributes["Level"].Value);

//					XmlNode infoNode = node.SelectSingleNode("Info");
//					string name = infoNode.SelectSingleNode("Name").InnerText;
//					string icon = infoNode.SelectSingleNode("Icon").InnerText;
//					string thumbnail = infoNode.SelectSingleNode("Thumbnail").InnerText;
//					string description = infoNode.SelectSingleNode("Description").InnerText;
//					bool allowAtRoot = bool.Parse(infoNode.SelectSingleNode("AllowAtRoot").InnerText);
//					bool isListView = bool.Parse(infoNode.SelectSingleNode("IsListView").InnerText);
//					string variations = infoNode.SelectSingleNode("Variations").InnerText;
//					bool isElement = bool.Parse(infoNode.SelectSingleNode("IsElement").InnerText);
//					string folder = infoNode.SelectSingleNode("Folder").InnerText;

//					IContentTypeComposition contentType = new ContentType(_shortStringHelper, -1)
//					{
//						Alias = contentTypeAlias,
//						Key = contentTypeKey,
//						Level = contentTypeLevel,
//						Name = name,
//						Icon = icon,
//						Thumbnail = thumbnail,
//						Description = description,
//						AllowedAsRoot = allowAtRoot,
//						IsContainer = isListView,
//						Variations = ContentVariation.Nothing,
//						IsElement = isElement,

//					};

//					// Add generic properties
//					XmlNodeList genericProperties = node.SelectNodes("GenericProperties/GenericProperty");
//					foreach (XmlNode propertyNode in genericProperties)
//					{
//						string propertyKey = propertyNode.SelectSingleNode("Key").InnerText;
//						string propertyName = propertyNode.SelectSingleNode("Name").InnerText;
//						string propertyAlias = propertyNode.SelectSingleNode("Alias").InnerText;
//						string propertyType = propertyNode.SelectSingleNode("Type").InnerText;
//						string propertyTabAlias = propertyNode.SelectSingleNode("Tab").InnerText;

//						PropertyType property = new PropertyType(_shortStringHelper, new DataType(propertyType), propertyAlias)
//						{
//							Name = propertyName,
//							SortOrder = int.Parse(propertyNode.SelectSingleNode("SortOrder").InnerText),
//						};

//						// Add the property to the specified tab
//						contentType.AddPropertyType(property, propertyTabAlias);
//					}

//					// Add tabs
//					XmlNodeList tabs = node.SelectNodes("Tabs/Tab");
//					foreach (XmlNode tabNode in tabs)
//					{
//						string tabKey = tabNode.SelectSingleNode("Key").InnerText;
//						string tabCaption = tabNode.SelectSingleNode("Caption").InnerText;
//						string tabAlias = tabNode.SelectSingleNode("Alias").InnerText;
//						string tabType = tabNode.SelectSingleNode("Type").InnerText;
//						int sortOrder = int.Parse(tabNode.SelectSingleNode("SortOrder").InnerText);

//						Tab tab = new Tab
//						{
//							Key = Guid.Parse(tabKey),
//							Caption = tabCaption,
//							Alias = tabAlias,
//							Type = tabType,
//							SortOrder = sortOrder
//						};

//						// Add the tab to the content type
//						contentType.AdditionalData["contentTypeComposition"]["tabs"].Add(tab);
//					}

//					// Save the content type
//					_contentService.Save(contentType);
//					Console.WriteLine($"Content type '{contentTypeAlias}' imported successfully.");
//				}

//				Console.WriteLine("XML import successful!");
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine($"Error importing XML: {ex.Message}");
//			}
//		}
//	}
//}
























//using System;
//using System.Collections.Generic;
//using System.Xml;
//using Umbraco.Cms.Core.Models;
//using Umbraco.Cms.Core.Models.ContentEditing;
//using Umbraco.Cms.Core.Services;
//using Umbraco.Cms.Core.Strings;

//namespace UmbracoDevelopmentProject
//{
//	public class ConfigImporter
//	{
//		private readonly IContentTypeService _contentService;
//		private readonly IShortStringHelper _shortStringHelper;
//		private readonly IDataType _dataTypeService;

//		public ConfigImporter(IContentTypeService contentService, IShortStringHelper shortStringHelper, IDataType dataTypeService)
//		{
//			_contentService = contentService;
//			_shortStringHelper = shortStringHelper;
//			_dataTypeService = dataTypeService;
//		}

//		public void ImportXml(string xmlFilePath)
//		{
//			try
//			{
//				XmlDocument xmlDoc = new XmlDocument();
//				xmlDoc.Load(xmlFilePath);

//				foreach (XmlNode node in xmlDoc.SelectNodes("/ContentType"))
//				{
//					Guid contentTypeKey = Guid.Parse(node.Attributes["Key"].Value);
//					string contentTypeAlias = node.Attributes["Alias"].Value;
//					int contentTypeLevel = int.Parse(node.Attributes["Level"].Value);

//					XmlNode infoNode = node.SelectSingleNode("Info");
//					string name = infoNode.SelectSingleNode("Name").InnerText;
//					string icon = infoNode.SelectSingleNode("Icon").InnerText;
//					string thumbnail = infoNode.SelectSingleNode("Thumbnail").InnerText;
//					string description = infoNode.SelectSingleNode("Description").InnerText;
//					bool allowAtRoot = bool.Parse(infoNode.SelectSingleNode("AllowAtRoot").InnerText);
//					string isListView = infoNode.SelectSingleNode("IsListView").InnerText;
//					string variationsValue = infoNode.SelectSingleNode("Variations").InnerText;
//					int variations = int.TryParse(variationsValue, out int parsedVariations) ? parsedVariations : 0;
//					bool isElement = bool.Parse(infoNode.SelectSingleNode("IsElement").InnerText);
//					string folder = infoNode.SelectSingleNode("Folder").InnerText;

//					IContentType contentType = new ContentType(_shortStringHelper, -1)
//					{
//						Alias = contentTypeAlias,
//						Key = contentTypeKey,
//						Level = contentTypeLevel,
//						Name = name,
//						Icon = icon,
//						Thumbnail = thumbnail,
//						Description = description,
//						AllowedAsRoot = allowAtRoot,
//						IsContainer = isListView == "List",
//						Variations = (ContentVariation)variations,
//						IsElement = isElement
//					};

//					// Extract Tabs
//					XmlNodeList tabs = node.SelectNodes("Tabs/Tab");
//					foreach (XmlNode tabNode in tabs)
//					{
//						string tabCaption = tabNode.SelectSingleNode("Caption").InnerText;
//						string tabAlias = tabNode.SelectSingleNode("Alias").InnerText;

//						// Create a new PropertyGroup
//						PropertyGroup propertyGroup = new PropertyGroup(true)
//						{
//							Name = tabCaption,
//							Alias = tabAlias,
//							SortOrder = 0 // You might need to set a proper sort order
//						};

//						// Extract properties under this tab
//						XmlNodeList properties = tabNode.SelectNodes("GenericProperties/GenericProperty");
//						foreach (XmlNode propertyNode in properties)
//						{
//							string propertyName = propertyNode.SelectSingleNode("Name").InnerText;
//							string propertyDescription = propertyNode.SelectSingleNode("Description").InnerText;
//							string propertyType = propertyNode.SelectSingleNode("Type").InnerText;
//							string propertyMandatory = propertyNode.SelectSingleNode("Mandatory").InnerText;
//							string propertySortOrder = propertyNode.SelectSingleNode("SortOrder").InnerText;
//							string propertyMandatoryMessage = propertyNode.SelectSingleNode("MandatoryMessage").InnerText;
//							string propertyValidationRegExpMessage = propertyNode.SelectSingleNode("ValidationRegExpMessage").InnerText;
//							string propertyLabelOnTop = propertyNode.SelectSingleNode("LabelOnTop").InnerText;

//							// Create a new property type and add it to the property group
//							PropertyType property = new PropertyType(_shortStringHelper, _dataTypeService)
//							{
//								Name = propertyName,
//								Variations = ContentVariation.Culture,
//								Description = propertyDescription,
//								Mandatory = bool.Parse(propertyMandatory),
//								SortOrder = int.Parse(propertySortOrder),
//								MandatoryMessage = propertyMandatoryMessage,
//								ValidationRegExpMessage = propertyValidationRegExpMessage,
//								LabelOnTop = bool.Parse(propertyLabelOnTop)
//							};

//							// Add the property type directly to the content type
//							contentType.AddPropertyType(property, propertyGroup.Alias, propertyGroup.Name);
//						}
//					}

//					// Save the content type after adding all property groups and properties
//					_contentService.Save(contentType);

//					// Additional logic to create or save content based on extracted data
//					Console.WriteLine($"Content type imported: {contentType.Name}");
//				}

//				Console.WriteLine("XML import successful!");
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine($"Error importing XML: {ex.Message}");
//			}
//		}
//	}
//}







//using System.Drawing;
//using System.Xml;
//using Umbraco.Cms.Core.Models;
//using Umbraco.Cms.Core.Models.ContentEditing;
//using Umbraco.Cms.Core.Services;
//using Umbraco.Cms.Core.Strings;

//namespace UmbracoDevelopmentProject
//{
//	public class ConfigImporter
//	{
//		private readonly IContentTypeService _contentService;
//		private readonly IShortStringHelper _shortStringHelper;
//		public ConfigImporter(IContentTypeService contentService, IShortStringHelper shortStringHelper)
//		{
//			_contentService = contentService;
//			_shortStringHelper = shortStringHelper;
//		}

//		public void ImportXml(string xmlFilePath)
//		{
//			try
//			{
//				XmlDocument xmlDoc = new XmlDocument();
//				xmlDoc.Load(xmlFilePath);

//				foreach (XmlNode node in xmlDoc.SelectNodes("/ContentType"))
//				{
//					Guid ContentTypeKey = Guid.Parse(node.Attributes["Key"].Value);
//					string ContentTypeAlias = node.Attributes["Alias"].Value;
//					int ContentTypeLevel = int.Parse(node.Attributes["Level"].Value);

//					XmlNode infoNode = node.SelectSingleNode("Info");
//					string Name = infoNode.SelectSingleNode("Name").InnerText;
//					string Icon = infoNode.SelectSingleNode("Icon").InnerText;
//					string Thumbnail = infoNode.SelectSingleNode("Thumbnail").InnerText;
//					string Description = infoNode.SelectSingleNode("Description").InnerText;
//					bool AllowAtRoot = bool.Parse(infoNode.SelectSingleNode("AllowAtRoot").InnerText);
//					string IsListView = infoNode.SelectSingleNode("IsListView").InnerText;
//					string variationsValue = infoNode.SelectSingleNode("Variations").InnerText;
//					int Variations = int.TryParse(variationsValue, out int variations) ? variations : 0;
//					bool IsElement = bool.Parse(infoNode.SelectSingleNode("IsElement").InnerText);
//					string Folder = infoNode.SelectSingleNode("Folder").InnerText;

//					IContentType contentType = new ContentType(_shortStringHelper, -1)
//					{
//						Alias = ContentTypeAlias,
//						Key = ContentTypeKey,
//						Level = ContentTypeLevel,
//						Name = Name,
//						Icon = Icon,
//						Thumbnail = Thumbnail,
//						Description = Description,
//						AllowedAsRoot = AllowAtRoot,
//						IsContainer = IsListView == "List",
//						Variations = (ContentVariation)Variations,
//						IsElement = IsElement,


//					};
//					_contentService.Save(contentType);





//					// Extract GenericProperties
//					XmlNodeList genericProperties = node.SelectNodes("GenericProperties/GenericProperty");
//					foreach (XmlNode propertyNode in genericProperties)
//					{
//						string propertyKey = propertyNode.SelectSingleNode("Key").InnerText;
//						string propertyName = propertyNode.SelectSingleNode("Name").InnerText;
//						string propertyAlias = propertyNode.SelectSingleNode("Alias").InnerText;
//						string propertyDefinition = propertyNode.SelectSingleNode("Definition").InnerText;
//						string propertyType = propertyNode.SelectSingleNode("Type").InnerText;
//						string propertyMandatory = propertyNode.SelectSingleNode("Mandatory").InnerText;
//						string propertyValidation = propertyNode.SelectSingleNode("Validation").InnerText;
//						string propertyDescription = propertyNode.SelectSingleNode("Description").InnerText;
//						string propertySortOrder = propertyNode.SelectSingleNode("SortOrder").InnerText;
//						string propertyTab = propertyNode.SelectSingleNode("Tab").InnerText;
//						string propertyVariation = propertyNode.SelectSingleNode("Variations").InnerText;
//						string propertyMandatoryMessage = propertyNode.SelectSingleNode("MandatoryMessage").InnerText;
//						string propertyValidationRegExpMessage = propertyNode.SelectSingleNode("ValidationRegExpMessage").InnerText;
//						string propertyLabelOnTop = propertyNode.SelectSingleNode("LabelOnTop").InnerText;
//						// Process other properties as needed



//					}


//					// Extract Tabs
//					XmlNodeList tabs = node.SelectNodes("Tabs/Tab");
//					foreach (XmlNode tabNode in tabs)
//					{
//						string tabKey = tabNode.SelectSingleNode("Key").InnerText;
//						string tabCaption = tabNode.SelectSingleNode("Caption").InnerText;
//						string tabAlias = tabNode.SelectSingleNode("Alias").InnerText;
//						string tabType = tabNode.SelectSingleNode("Type").InnerText;


//						// Process other properties as needed
//					}


//					// Additional logic to create or save content based on extracted data
//					Console.WriteLine($"Content type imported:");
//				}

//				Console.WriteLine("XML import successful!");
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine($"Error importing XML: {ex.Message}");
//			}
//		}
//	}
//}


