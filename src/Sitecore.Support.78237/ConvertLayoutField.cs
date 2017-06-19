using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Save;
using System;

namespace Sitecore.Support.Pipelines.Save
{
    public class ConvertLayoutField
    {
        public void Process(SaveArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.IsNotNull(args.Items, "args.Items");
            SaveArgs.SaveItem[] items = args.Items;
            for (int i = 0; i < items.Length; i++)
            {
                SaveArgs.SaveItem saveItem = items[i];
                Item item = Client.ContentDatabase.Items[saveItem.ID, saveItem.Language, saveItem.Version];
                if (item != null)
                {
                    SaveArgs.SaveField[] fields = saveItem.Fields;
                    for (int j = 0; j < fields.Length; j++)
                    {
                        SaveArgs.SaveField saveField = fields[j];
                        Field field = item.Fields[saveField.ID];
                        if (!field.IsBlobField && field.Type == "layout")
                        {
                            string value = field.Value;
                            string value2 = saveField.Value;
                            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(value2) && !value.Equals(value2))
                            {
                                saveField.Value = this.GetDeltaFieldValue(field, value2);
                            }
                        }
                    }
                }
            }
        }

        private string GetDeltaFieldValue(Field field, string value)
        {
            Assert.ArgumentNotNull(field, "field");
            Assert.ArgumentNotNull(value, "value");
            Assert.IsTrue(field.ID == FieldIDs.LayoutField || field.ID == FieldIDs.FinalLayoutField, "The field is not a layout/renderings field");
            string text = null;
            bool flag = field.Item.Name == "__Standard Values";
            bool flag2 = field.ID == FieldIDs.LayoutField;
            Field field2;
            if (flag && flag2)
            {
                field2 = null;
            }
            else if (flag)
            {
                field2 = field.Item.Fields[FieldIDs.LayoutField];
            }
            else if (flag2)
            {
                TemplateItem template = field.Item.Template;
                field2 = ((template != null && template.StandardValues != null) ? template.StandardValues.Fields[FieldIDs.FinalLayoutField] : null);
            }
            else
            {
                field2 = field.Item.Fields[FieldIDs.LayoutField];
            }

            if (field2 != null)
            {
                text = LayoutField.GetFieldValue(field2);
            }
            if (!string.IsNullOrWhiteSpace(text))
            {
                return XmlDeltas.GetDelta(value, text);
            }
            return value;
        }
    }
}
