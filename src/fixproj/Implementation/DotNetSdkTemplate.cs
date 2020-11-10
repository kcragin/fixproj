﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using fixproj.Abstract;

namespace fixproj.Implementation
{
    public class DotNetSdkTemplate : BaseTemplate, IOperateOnProjectFiles
    {
        public IList<string> Changes { get; }

        public XDocument ModifiedDocument { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetSdkTemplate"/> class.
        /// </summary>
        /// <param name="file">The path of the processed file.</param>
        public DotNetSdkTemplate(string file)
        {
            ModifiedDocument = XDocument.Load(file);
            Changes = new List<string>();

            Initialize(ModifiedDocument);
        }

        /// <inheritdoc />
        public List<ItemGroupEntity> FixContent()
        {
            ItemGroupElements.ForEach(x => x.Remove());
            FixPropertyGroups(ModifiedDocument.Root, Changes);

            // exclude elements which contain None local name and Remove attribute
            // these elements will be populated only if csproj file contains EmbeddedResources
            ItemGroupElements.Elements().Where(element => element.Name.LocalName.Equals(Constants.NoneNode) 
                                                          && !string.IsNullOrWhiteSpace(element.AttributeValueByName(Constants.RemoveAttribute))).Remove();

            var itemGroupElement = new XElement(Constants.ItemGroupNode);
            ItemGroupElements.Elements().ForEach(element =>
            {
                var originalCaseIncludeValue = element.AttributeValueByName(Constants.IncludeAttribute);

                if (element.HasNoContent())
                {
                    Changes.Add($"{element.Name.LocalName}: removing all empty content from {originalCaseIncludeValue}");
                    element.MakeEmpty();
                }

                if(string.IsNullOrWhiteSpace(originalCaseIncludeValue))
                    return;
                    
                if(element.Name.LocalName.Equals(Constants.EmbeddedResourceNode) || element.Name.LocalName.Equals(Constants.ContentNode))
                {
                    itemGroupElement.Add(new XElement(Constants.NoneNode,
                        new XAttribute(Constants.RemoveAttribute, element.AttributeValueByName(Constants.IncludeAttribute))));
                }
            });

            if(!itemGroupElement.HasNoContent())
                ItemGroupElements.Add(itemGroupElement);

            return ItemGroupElements
                .SelectMany(x => x.Elements())
                .ToLookup(x => x.Name)
                .OrderBy(x => x.Key.LocalName)
                .Select(x => new ItemGroupEntity { LocalName = x.Key.LocalName, Element = new List<XElement>(x) })
                .ToList();
        }

        /// <inheritdoc />
        public void DeleteDuplicates(ItemGroupEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var attributeName = entity.LocalName.Equals(Constants.NoneNode)
                ? Constants.RemoveAttribute
                : Constants.IncludeAttribute;

            DeleteDuplicatesBasedOnAttribute(entity, Changes, x => x.AttributeValueByName(attributeName));
        }

        /// <inheritdoc />
        public void DeleteReferencesToNonExistentFiles(ItemGroupEntity entity)
        {
            //exclude elements which contain EmbeddedResource local name, its attribute contains .resx extension, and they are part of this project
            //by default, the new sdk will pick up default globbing pattern <EmbeddedResource Include="**\*.resx" />
            ItemGroupElements.Elements()
                .Where(element => element.Name.LocalName.Equals(Constants.EmbeddedResourceNode) 
                                  && element.HasAttributes && !string.IsNullOrWhiteSpace(element.IfAttributesContainExtension(".resx")))
                .ForEach(x => entity.Element.Remove(x));

            // exclude elements which contain Compile local name
            // By default, the new SDK will pick up default globbing patterns <Compile Include="**\*.cs" />
            ItemGroupElements.Elements().Where(element => element.Name.LocalName.Equals(Constants.CompileNode)).ForEach(x => entity.Element.Remove(x));
        }

        /// <inheritdoc />
        public void MergeAndSortItemGroups(ItemGroupEntity entity, bool sort)
        {
            var groupToAdd = new XElement(Constants.ItemGroupNode);

            if (sort)
            {
                groupToAdd.Add(entity.Element.OrderBy(x => x.AttributeValueByName(Constants.IncludeAttribute)));
                Changes.Add($"{entity.LocalName}: sorted");
            }
            else
            {
                groupToAdd.Add(entity.Element);
            }

            InsertedAt.AddAfterSelf(groupToAdd);
            InsertedAt = groupToAdd;
        }

        /// <inheritdoc />
        public void SortPropertyGroups() => Sort(ModifiedDocument);
    }
}