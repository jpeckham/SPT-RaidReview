using EFT.UI;
using BepInEx;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace RAID_REVIEW
{
    public class MenuTaskbarMod : MonoBehaviour
    {
        public MenuTaskbarMod(IntPtr pointer) : base(pointer) { }

        public static Boolean Insert(MenuTaskBar instance)
        {
            List<ToggleGroup> toggleGroups = new List<ToggleGroup>(UnityEngine.Object.FindObjectsOfType<ToggleGroup>());

            ToggleGroup toggleGroupExists = toggleGroups.Find((toggleGroupItem) => toggleGroupItem.name == "RaidReview");
            if (toggleGroupExists != null)
            {
                return false;
            }

            foreach (var toggleGroup in toggleGroups)
            {
                if (toggleGroup != null && toggleGroup.name == "Handbook")
                {
                    ToggleGroup duplicatedToggleGroup = Instantiate(toggleGroup, toggleGroup.transform.parent);

                    duplicatedToggleGroup.name = "RaidReview";

                    HoverTooltipArea tooltipArea = duplicatedToggleGroup.GetComponent<HoverTooltipArea>();
                    tooltipArea?.SetUnlockStatus(true);

                    for (int childIndex = duplicatedToggleGroup.transform.childCount - 1; childIndex >= 0; childIndex--)
                    {
                        var child = duplicatedToggleGroup.transform.GetChild(childIndex);
                        if (child.name == "NewInformation")
                        {
                            Destroy(child.gameObject);
                        }

                        else
                        {
                            child.name = "RaidReviewButton";

                            AnimatedToggle childAnimatedToggle = child.GetComponent<AnimatedToggle>();
                            if (childAnimatedToggle != null)
                            {
                                childAnimatedToggle.onValueChanged.AddListener(Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.Events.UnityAction<bool>>((Action<bool>)((value) =>
                                {
                                    if (value)
                                    {
                                        Application.OpenURL(RAID_REVIEW.RAID_REVIEW_HTTP_Server);
                                        childAnimatedToggle.isOn = false;
                                    }
                                })));
                            }

                            for (int subIndex = 0; subIndex < child.childCount; subIndex++)
                            {
                                var subChild = child.GetChild(subIndex);
                                if (subChild.name == "Text")
                                {
                                    LocalizedText subChildEftText = subChild.GetComponent<LocalizedText>();
                                    if (subChildEftText != null)
                                    {

                                        if (subChildEftText.LocalizationKey == "HANDBOOK")
                                        {
                                            subChildEftText.LocalizationKey = "RAID REVIEW";
                                        }

                                        // Use reflection to update all public string fields and properties
                                        Type type = subChildEftText.GetType();
                                        PropertyInfo[] properties = type.GetProperties();
                                        FieldInfo[] fields = type.GetFields();

                                        foreach (var property in properties)
                                        {
                                            if (property.PropertyType == typeof(string) && property.CanWrite)
                                            {
                                                string value = (string)property.GetValue(subChildEftText);
                                                if (value == "HANDBOOK")
                                                {
                                                    property.SetValue(subChildEftText, "RAID REVIEW");
                                                }
                                            }
                                        }

                                        foreach (var field in fields)
                                        {
                                            if (field.FieldType == typeof(string))
                                            {
                                                string value = (string)field.GetValue(subChildEftText);
                                                if (value == "HANDBOOK")
                                                {
                                                    field.SetValue(subChildEftText, "RAID REVIEW");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    int siblingIndex = toggleGroup.transform.GetSiblingIndex();

                    List<ToggleGroup> toggleGroupsReEval = new List<ToggleGroup>(UnityEngine.Object.FindObjectsOfType<ToggleGroup>());
                    ToggleGroup newToggleGroupExistsReval = toggleGroupsReEval.Find((toggleGroupItem) => toggleGroupItem.name == duplicatedToggleGroup.name);
                    if (newToggleGroupExistsReval == null)
                    {
                        duplicatedToggleGroup.transform.SetSiblingIndex(siblingIndex + 2);
                    }
                    else
                    {
                        newToggleGroupExistsReval.transform.SetSiblingIndex(siblingIndex + 2);
                    }

                    

                    break;
                }
            }

            return true;
        }
    }
}

