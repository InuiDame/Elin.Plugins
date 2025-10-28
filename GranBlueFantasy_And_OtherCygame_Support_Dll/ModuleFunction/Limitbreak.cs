using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using Cwl.API.Custom;
using Cwl.Helper.Unity;
using Cwl.LangMod;
using GBF.Modinfo;
using HarmonyLib;
using Limitbreak;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore;
using Object = UnityEngine.Object;

namespace Limitbreak
{
    /// 这段代码的原理是创造一个UI和一个升级项目，并通过访问元素记录经验的方式为武器提供伪升级的效果，代码通过读取元素的tag去避免将一些元素加入升级池，因为有些元素被引用到武器上后会发生意外错误。
    /// The principle of this code is to create a UI and an upgrade item, providing a pseudo-upgrade effect for weapons by accessing element experience records. The code reads element tags to prevent certain elements from being added to the upgrade pool, as referencing some elements on weapons may cause unexpected errors.
    /// このコードの原理は、UIとアップグレード項目を作成し、エレメントの経験値記録にアクセスすることで武器に擬似的なアップグレード効果を提供することです。コードはエレメントのタグを読み取って特定のエレメントがアップグレードプールに追加されるのを防ぎます。なぜなら、一部のエレメントが武器で参照されると予期しないエラーが発生する可能性があるためです。

    [HarmonyPatch]
    internal class AttackProcessPerformPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AttackProcess), "Perform")]
        internal static void Postfix(ref AttackProcess __instance)
        {
            //Debug.Log("进入Perform Postfix ");

            var tC = __instance.TC;
            var weapon = __instance.weapon;

            if (tC == null || weapon == null)
                return;

            const int elementId = 170022;
            if (!weapon.HasElement(elementId))
            {
                //Debug.Log($"[GBF] 武器上没有元素 {elementId}，跳过经验增长");
                return;
            }
            var element = weapon.elements.GetElement(elementId);
            if (element == null)
            {
                Debug.LogWarning($"[GBF] 虽然 HasElement({elementId}) 为 true，但 GetElement 却返回了 null？");
                return;
            }
            
            
            if (__instance.TC.IsAliveInCurrentZone)
            {
                
                return;
            }
            var baseElemW = weapon.elements.GetElement("GBFlimitbeark");
            int baseValW = baseElemW?.vBase ?? 0;
            if (baseValW == 0)
            {
                Debug.LogError("[GBF] GBFlimitbeark.vBase == 0 on weapon, forcing to 1");
                baseValW = 1;
            }
            int gain = Rand.rnd(__instance.TC.LV / baseValW) + 1;
            element.vExp += gain;
            //Debug.Log($"[GBF] 给武器元素 {elementId} 加经验 {gain}，当前 vExp={element.vExp}/{element.ExpToNext}");
            Msg.SayRaw(weapon.GetName(NameStyle.Full) + "获得经验" + gain + "当前经验：" + element.vExp + "/" + element.ExpToNext);
            if (element.vExp >= element.ExpToNext)
            {
                element.vExp = element.ExpToNext;
                //Debug.Log("[GBF] 元素经验已满级");
                Msg.SayRaw(weapon.GetName(NameStyle.Full) + "GBFlimitbeark_Growth2".lang());
            }
            
            
            if (!GBF_and_PCR_Equipment.globalExp.Value || tC.IsPC)
            {
                
                return;
            }

            var attacker = __instance.CC;
            if (attacker == null || attacker.body == null || attacker.body.slots == null)
                return;
            foreach (var slot in attacker.body.slots)
            {
                // 逐个 slot 再检查
                if (slot == null)
                    continue;

                var thing = slot.thing;
                if (thing == null || thing.elements == null)
                    continue;

                if (!thing.HasElement(elementId))
                    continue;

                var elem2 = thing.elements.GetElement(elementId);
                if (elem2 == null)
                    continue;

                // 再检查它的 base
                var baseElemT = thing.elements.GetElement("GBFlimitbeark");
                int baseValT = baseElemT?.vBase ?? 0;
                if (baseValT == 0) baseValT = 1;

                int gain2 = Rand.rnd(__instance.TC.LV / baseValT) + 1;
                elem2.vExp += gain2;
                if (elem2.vExp >= elem2.ExpToNext)
                {
                    elem2.vExp = elem2.ExpToNext;
                    Msg.SayRaw(thing.GetName(NameStyle.Full) + "GBFlimitbeark_Growth2".lang());
                }
            }
            
            //Debug.Log($"HasElement({elementId}) = {weapon.HasElement(elementId)}; ExpToNext = {weapon.elements.GetElement(elementId)?.ExpToNext}");
            return;
        }
    }

    [HarmonyPatch]
    internal class ProcAbsorbPatch
    {
        internal static MethodBase TargetMethod()
        {
            Type typeFromHandle = typeof(Card);
            Type[] nestedTypes = typeFromHandle.GetNestedTypes(BindingFlags.NonPublic);
            Type[] array = nestedTypes;
            foreach (Type type in array)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo[] array2 = methods;
                foreach (MethodInfo methodInfo in array2)
                {
                    if (methodInfo.Name.Contains("ProcAbsorb"))
                    {
                        return methodInfo;
                    }
                }
            }
            return null;
        }

        [HarmonyPostfix]
        internal static void Postfix(object __instance)
        {
            try
            {
                Traverse traverse = Traverse.Create(__instance);

                // 安全获取字段值
                object originObj = traverse.Field("origin").GetValue();
                object attackSourceObj = traverse.Field("attackSource").GetValue();
                object dmgObj = traverse.Field("dmg").GetValue();
                object weaponObj = traverse.Field("weapon").GetValue();
                object thisObj = traverse.Field("<>4__this").GetValue();

                if (!(originObj is Card origin) || !(thisObj is Card currentCard))
                {
                    return;
                }

                if (!origin.isChara || !currentCard.isChara)
                {
                    return;
                }

                // 注意：dmg 现在应该是 long 类型
                AttackSource attackSource = attackSourceObj as AttackSource? ?? AttackSource.None;
                long dmg = Convert.ToInt64(dmgObj); // 转换为 long
                Thing weapon = weaponObj as Thing;

                // 修复：将所有的 DamageHP 调用改为使用 long
                int num = origin.Evalue(660) + (weapon?.Evalue(660, ignoreGlobalElement: true) ?? 0);

                if (num > 0 && attackSource == AttackSource.Melee)
                {
                    int num2 = EClass.rnd(2 + Mathf.Clamp((int)(dmg / 10), 0, num + 10)); // 注意类型转换
                    origin.Chara.HealHP(num2);
                    if (currentCard.IsAliveInCurrentZone)
                    {
                        // 修复：使用 long 类型
                        currentCard.Chara.DamageHP((long)num2);
                    }
                }

                int num3 = origin.Evalue(170007) + (weapon?.Evalue(170007, ignoreGlobalElement: true) ?? 0);

                if (num3 > 0 && attackSource == AttackSource.Melee)
                {
                    int num4 = EClass.rnd(2 + Mathf.Clamp((int)(dmg / 100), 0, num3 + 10)); // 注意类型转换
                    origin.Chara.HealHP(num4);
                    if (currentCard.IsAliveInCurrentZone)
                    {
                        // 修复：使用 long 类型
                        currentCard.Chara.DamageHP((long)num4);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ProcAbsorbPatch error: {ex}");
            }
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            for (int i = 0; i < list.Count - 2; i++)
            {
                if (list[i].opcode == OpCodes.Callvirt && list[i].operand.ToString().Contains("get_Chara()") && list[i + 1].opcode == OpCodes.Ldfld && list[i + 1].operand.ToString().Contains("ignoreSPAbsorb") && list[i + 2].opcode == OpCodes.Brtrue)
                {
                    list[i] = new CodeInstruction(OpCodes.Pop);
                    list[i + 1] = new CodeInstruction(OpCodes.Ldc_I4_0);
                    return list;
                }
            }
            return list;
        }
    }

    [HarmonyPatch]
    internal class ThingOnCreatePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Thing), "OnCreate")]
        internal static void Postfix(ref Thing __instance)
        {

            //Debug.Log("进入Thing Postfix ");
            if ((__instance.rarity != Rarity.Legendary && __instance.rarity != Rarity.Mythical) || (!__instance.IsMeleeWeapon && !__instance.IsRangedWeapon))
            {
         
                return;
            }
            int num = GBF_and_PCR_Equipment.elementRarity1.Value;
            if (num < 1)
            {
                num = 200;
            }
     
            if (Rand.rnd(num) != 0)
            {
 
                return;
            }
            List<SourceElement.Row> list = new List<SourceElement.Row>();
            foreach (SourceElement.Row row in EClass.sources.elements.rows)
            {

                if (row.alias.Contains("GBFlimitbeark"))
                {
                    list.Add(row);
                }

            }
            foreach (SourceElement.Row item in list)
            {

                //Debug.Log("创造物品成功");
                Tuple<SourceElement.Row, int> tuple = new Tuple<SourceElement.Row, int>(item, 1);
                __instance.elements.ModBase(tuple.Item1.id, tuple.Item2);
            }
        }
    }

    // 拦截 Element.ExpToNext 的 getter
    [HarmonyPatch(typeof(Element))]
    [HarmonyPatch("get_ExpToNext")]   // 或者 nameof(Element.ExpToNext) + MethodType.Getter
    static class Patch_Element_ExpToNext
    {
        // 在原逻辑执行前拦截，直接返回我们自己算好的值
        [HarmonyPrefix]
        static bool Prefix(Element __instance, ref int __result)
        {
            
            const int MyElementId = 170022;
            if (__instance.id == MyElementId)
            {
                // 用 vBase 当“等级”，每一级 +200
                int lvl = __instance.vBase;
                __result = 1000 + lvl * 200;
                //Debug.Log($"经验上限修改为{__result}" );
                return false;    // 阻止原方法运行

            }
            // 不是目标元素，走原逻辑
            return true;
        }
    }







}



namespace limitbreakUI
{

    public static class Ext
    {
        public static List<Dropdown.OptionData> ToDropdownOptions(this List<string> list)
        {
            return list.Select((string x) => new Dropdown.OptionData(x)).ToList();
        }

        public static string _(this string ja, string en = "", string cn = "")
        {
            if (Lang.isJP)
            {
                return ja;
            }
            if (Lang.isEN)
            {
                return en;
            }
            return cn;
        }

        public static void DestroyAllChildren(this Transform parent)
        {
            foreach (Transform item in parent)
            {
                item.gameObject.SetActive(value: false);
                UnityEngine.Object.Destroy(item.gameObject);
                item.SetActive(enable: false);
                UnityEngine.Object.Destroy(item);
            }
        }

        public static T2 ReplaceComponent<T, T2>(this T original) where T : MonoBehaviour where T2 : MonoBehaviour
        {
            GameObject gameObject = original.gameObject;
            for (int i = 0; i < original.transform.childCount; i++)
            {
                original.transform.GetChild(i).gameObject.SetActive(value: false);
            }
            UnityEngine.Object.Destroy(original);
            gameObject.name = typeof(T2).Name;
            T2 val = gameObject.AddComponent<T2>();
            try
            {
                FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                FieldInfo[] array = fields;
                foreach (FieldInfo fieldInfo in array)
                {
                    fieldInfo.SetValue(val, fieldInfo.GetValue(original));
                }
            }
            catch (Exception)
            {
            }
            return val;
        }

        public static LayoutElement LayoutElement(this Component component)
        {
            return component.GetOrCreate<LayoutElement>();
        }

        public static void DestroyObject(this Component component)
        {
            UnityEngine.Object.Destroy(component.gameObject);
        }

        public static T ReplaceLayerComponent<T>(this Layer original) where T : MonoBehaviour
        {
            GameObject gameObject = original.gameObject;
            T val = gameObject.AddComponent<T>();
            try
            {
                FieldInfo[] fields = typeof(Layer).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                FieldInfo[] array = fields;
                foreach (FieldInfo fieldInfo in array)
                {
                    fieldInfo.SetValue(val, fieldInfo.GetValue(original));
                }
                UnityEngine.Object.Destroy(original);
                gameObject.name = typeof(T).Name;
            }
            catch (Exception)
            {
            }
            return val;
        }

        public static Window AddWindow(this Layer layer, Window.Setting setting)
        {
            if (setting.tabs == null)
            {
                setting.tabs = new List<Window.Setting.Tab>();
            }
            LayerList layerList = (LayerList)Layer.Create(typeof(LayerList).Name);
            Window window = layerList.windows.First();
            window.transform.SetParent(layer.transform);
            layer.windows.Add(window);
            UnityEngine.Object.Destroy(layerList.gameObject);
            window.setting = setting;
            window.RectTransform.sizeDelta = setting.bound.size;
            window.RectTransform.position = setting.bound.position;
            DestroyAllChildren(window.Find("Content View"));
            return window;
        }

        public static UIInputText WithPlaceholder(this UIInputText input, string text)
        {
            Transform transform = input.Find("Placeholder");
            UnityEngine.UI.Text component = transform.GetComponent<UnityEngine.UI.Text>();
            transform.SetActive(enable: true);
            component.text = text;
            return input;
        }

        public static T WithName<T>(this T component, string text) where T : Component
        {
            component.gameObject.name = text;
            return component;
        }

        public static T WithWidth<T>(this T component, int size) where T : Component
        {
            component.GetOrCreate<LayoutElement>().preferredWidth = size;
            return component;
        }

        public static T WithMinWidth<T>(this T component, int size) where T : Component
        {
            component.GetOrCreate<LayoutElement>().minWidth = size;
            return component;
        }

        public static T WithHeight<T>(this T component, int size) where T : Component
        {
            component.GetOrCreate<LayoutElement>().preferredHeight = size;
            return component;
        }

        public static T WithMinHeight<T>(this T component, int size) where T : Component
        {
            component.GetOrCreate<LayoutElement>().minHeight = size;
            return component;
        }

        public static T WithPivot<T>(this T component, float x, float y) where T : Component
        {
            component.Rect().pivot = new Vector2(x, y);
            return component;
        }

        public static T WithLayerParent<T>(this T component) where T : Component
        {
            try
            {
                component.transform.SetParent(component.transform.GetComponentInParent<ELayer>().transform);
            }
            catch
            {
            }
            component.SetActive(enable: false);
            return component;
        }
    }
    public static class GBF_MaybeItWillGone
    {
        public static Dictionary<Type, UnityEngine.Object> UIObjects = new Dictionary<Type, UnityEngine.Object>();

        public static T GetResource<T>(string hint) where T : Component
        {
            if (!UIObjects.TryGetValue(typeof(T), out var value))
            {
                value = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault((T x) => x.name == hint);
                UIObjects.Add(typeof(T), value);
                if (value == null)
                {
                    Debug.Log("无法实例化资源： " + hint);
                }
            }
            return (T)UnityEngine.Object.Instantiate(value);
        }

        public static T Create<T>(Transform? parent = null) where T : MonoBehaviour
        {
            GameObject gameObject = new GameObject(typeof(T).Name, typeof(RectTransform));
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            return gameObject.AddComponent<T>();
        }

        public static T CreateLayer<T, T2>(T2 arg) where T : ABCDEFGLayer<T2>
        {
            T val = EMono.ui.layers.Find((Layer o) => o.GetType() == typeof(T)) as T;
            if (val != null)
            {
                val.SetActive(enable: true);
                return val;
            }
            T val2 = Create<T>();
            T val3 = UnityEngine.Object.Instantiate(val2);
            val3.gameObject.name = typeof(T).Name;
            val2.DestroyObject();
            val3.Data = arg;
            val3.AddWindow(new Window.Setting
            {
                textCaption = val3.Title,
                bound = val3.Bound,
                allowMove = true,
                transparent = false,
                openLastTab = false
            });
            val3.OnLayout();
            EMono.ui.AddLayer(val3);
            return val3;
        }

        public static T CreateLayer<T>() where T : ABCDEFGLayer<object>
        {
            return CreateLayer<T, object>(0);
        }
    }

    public class ABCDEFGHorizontal : ABCDEFGLayout
    {
        protected HorizontalLayoutGroup? _layout;

        protected ContentSizeFitter? _fitter;

        public HorizontalLayoutGroup Layout => _layout;

        public ContentSizeFitter Fitter => _fitter;

        public override void OnLayout()
        {
            HorizontalLayoutGroup horizontalLayoutGroup = base.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childControlHeight = false;
            horizontalLayoutGroup.childForceExpandHeight = false;
            horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childForceExpandWidth = false;
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            _layout = horizontalLayoutGroup;
            ContentSizeFitter contentSizeFitter = base.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            _fitter = contentSizeFitter;
        }
    }

    public abstract class ABCDEFGLayer<T> : ELayer
    {
        protected T? _data;

        public virtual string Title { get; } = "ウィンドウ"._("Window","窗口");

        public virtual Rect Bound { get; } = new Rect(0f, 0f, 640f, 480f);

        public Window Window => windows[0];

        public T Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public override bool blockWidgetClick => false;

        public virtual void OnLayout()
        {
        }

        public L CreateTab<L>(string idLang, string id) where L : ABCDEFGLayout<T>
        {
            Transform transform = Window.Find("Content View");
            transform.gameObject.GetComponent<RectTransform>();
            if (!GBF_MaybeItWillGone.UIObjects.ContainsKey(typeof(ScrollRect)))
            {
                Resources.Load("UI/Layer/LayerAnnounce");
            }
            ScrollRect resource = GBF_MaybeItWillGone.GetResource<ScrollRect>("Scrollview parchment with Header");
            resource.gameObject.name = id;
            RectTransform rectTransform = resource.Rect();
            rectTransform.SetParent(transform);
            UIContent orCreate = resource.GetOrCreate<UIContent>();
            ((RectTransform)rectTransform.Find("Header Top Parchment")).SetActive(enable: false);
            RectTransform rectTransform2 = (RectTransform)((RectTransform)rectTransform.Find("Viewport")).Find("Content");
            rectTransform2.DestroyAllChildren();
            VerticalLayoutGroup component = rectTransform2.gameObject.GetComponent<VerticalLayoutGroup>();
            component.childControlHeight = true;
            component.padding = new RectOffset(5, 5, 5, 5);
            L val = GBF_MaybeItWillGone.Create<L>(rectTransform2);
            val.gameObject.name = id;
            val.GetComponent<RectTransform>();
            VerticalLayoutGroup verticalLayoutGroup = val.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.padding = new RectOffset(10, 10, 0, 10);
            val.Layer = this;
            val.OnLayout();
            val.RebuildLayout(recursive: true);
            Window.AddTab(idLang, orCreate);
            return val;
        }

        public override void OnBeforeAddLayer()
        {
            option.rebuildLayout = true;
        }

        public override void OnAfterAddLayer()
        {
            foreach (Window window in windows)
            {
                window.Rect();
                window.RectTransform.localPosition = new Vector3(window.setting.bound.x, window.setting.bound.y, 0f);
            }
        }
    }
    public class ABCDEFGLayout : UIContent
    {
        public virtual void OnLayout()
        {
        }

        public RectTransform Spacer(int height, int width = 1)
        {
            Transform transform = Util.Instantiate<Transform>("UI/Element/Deco/Space", layout);
            transform.SetParent(base.transform);
            RectTransform rectTransform = transform.Rect();
            rectTransform.sizeDelta = new Vector2(width, height);
            if (height != 1)
            {
                rectTransform.LayoutElement().preferredHeight = height;
            }
            if (width != 1)
            {
                rectTransform.LayoutElement().preferredWidth = width;
            }
            return rectTransform;
        }

        public UIItem Header(string text, Sprite? sprite = null)
        {
            UIItem uIItem = AddHeader(text, sprite);
            uIItem.transform.SetParent(base.transform);
            return uIItem;
        }

        public UIItem HeaderCard(string text, Sprite? sprite = null)
        {
            UIItem uIItem = AddHeaderCard(text, sprite);
            uIItem.transform.SetParent(base.transform);
            return uIItem;
        }

        public UIItem HeaderSmall(string text, Sprite? sprite = null)
        {
            UIItem uIItem = AddHeader("HeaderNoteSmall", text, sprite);
            uIItem.transform.SetParent(base.transform);
            return uIItem;
        }

        public UIText Text(string text, FontColor color = FontColor.DontChange)
        {
            UIItem uIItem = AddText(text, color);
            uIItem.transform.SetParent(base.transform);
            uIItem.text1.horizontalOverflow = HorizontalWrapMode.Wrap;
            uIItem.GetOrCreate<LayoutElement>().minWidth = 80f;
            return uIItem.GetComponent<UIText>();
        }

        public UIText TextLong(string text, FontColor color = FontColor.DontChange)
        {
            UIItem uIItem = AddText("NoteText_long", text, color);
            uIItem.transform.SetParent(base.transform);
            uIItem.text1.horizontalOverflow = HorizontalWrapMode.Wrap;
            uIItem.GetOrCreate<LayoutElement>().minWidth = 80f;
            return uIItem.GetComponent<UIText>();
        }

        public UIText TextMedium(string text, FontColor color = FontColor.DontChange)
        {
            UIItem uIItem = AddText("NoteText_medium", text, color);
            uIItem.transform.SetParent(base.transform);
            uIItem.text1.horizontalOverflow = HorizontalWrapMode.Wrap;
            uIItem.GetOrCreate<LayoutElement>().minWidth = 80f;
            return uIItem.GetComponent<UIText>();
        }

        public UIText TextSmall(string text, FontColor color = FontColor.DontChange)
        {
            UIItem uIItem = AddText("NoteText_small", text, color);
            uIItem.transform.SetParent(base.transform);
            uIItem.text1.horizontalOverflow = HorizontalWrapMode.Wrap;
            uIItem.GetOrCreate<LayoutElement>().minWidth = 80f;
            return uIItem.GetComponent<UIText>();
        }

        public UIText TextFlavor(string text, FontColor color = FontColor.DontChange)
        {
            UIItem uIItem = AddText("NoteText_flavor", text, color);
            uIItem.transform.SetParent(base.transform);
            uIItem.GetOrCreate<LayoutElement>().minWidth = 80f;
            return uIItem.GetComponent<UIText>();
        }

        public UIItem Topic(string text, string? value = null)
        {
            UIItem uIItem = AddTopic("TopicDefault", text, value);
            uIItem.transform.SetParent(base.transform);
            return uIItem;
        }

        public UIItem TopicAttribute(string text, string? value = null)
        {
            UIItem uIItem = AddTopic("TopicAttribute", text, value);
            uIItem.transform.SetParent(base.transform);
            return uIItem;
        }

        public UIItem TopicDomain(string text, string? value = null)
        {
            UIItem uIItem = AddTopic("TopicDomain", text, value);
            uIItem.transform.SetParent(base.transform);
            return uIItem;
        }

        public UIItem TopicLeft(string text, string? value = null)
        {
            UIItem uIItem = AddTopic("TopicLeft", text, value);
            uIItem.transform.SetParent(base.transform);
            return uIItem;
        }

        public UIItem TopicPair(string text, string? value = null)
        {
            UIItem uIItem = AddTopic("TopicPair", text, value);
            uIItem.transform.SetParent(base.transform);
            return uIItem;
        }

        public UIButton Button(string text, Action action)
        {
            UIButton uIButton = AddButton(text, delegate
            {
                SE.ClickGeneral();
                action();
            });
            uIButton.transform.SetParent(base.transform);
            uIButton.GetOrCreate<LayoutElement>().minWidth = 80f;
            return uIButton;
        }

        public UIButton Toggle(string text, bool isOn = false, Action<bool>? onClick = null)
        {
            UIButton uIButton = AddToggle(text, isOn, onClick);
            uIButton.transform.SetParent(base.transform);
            return uIButton;
        }

        public UISlider Slider<TValue>(int index, IList<TValue> list, Action<int, TValue> onChange, Func<TValue, string>? getInfo = null)
        {
            if (!GBF_MaybeItWillGone.UIObjects.ContainsKey(typeof(UISlider)))
            {
                LayerEditPCC layerEditPCC = Layer.Create<LayerEditPCC>();
                GBF_MaybeItWillGone.UIObjects.Add(typeof(UISlider), UnityEngine.Object.Instantiate(layerEditPCC.sliderPortrait));
                UnityEngine.Object.Destroy(layerEditPCC.gameObject);
            }
            UISlider uISlider = (UISlider)UnityEngine.Object.Instantiate(GBF_MaybeItWillGone.UIObjects[typeof(UISlider)]);
            uISlider.Rect().SetParent(base.transform);
            uISlider.SetList(index, list, onChange, getInfo);
            uISlider.textMain.text = "";
            uISlider.textInfo.text = "";
            return uISlider;
        }

        public Slider Slider(float value, Action<float> setvalue, float min, float max, Func<float, string>? labelfunc = null)
        {
            if (!GBF_MaybeItWillGone.UIObjects.ContainsKey(typeof(Slider)))
            {
                LayerConfig layerConfig = Layer.Create<LayerConfig>();
                GBF_MaybeItWillGone.UIObjects.Add(typeof(Slider), UnityEngine.Object.Instantiate(layerConfig.sliderBGM));
                UnityEngine.Object.Destroy(layerConfig.gameObject);
            }
            Slider slider = (Slider)UnityEngine.Object.Instantiate(GBF_MaybeItWillGone.UIObjects[typeof(Slider)]);
            slider.Rect().SetParent(base.transform);
            Func<float, string> labelfunc2 = labelfunc;
            slider.SetSlider(value, delegate (float v)
            {
                string result = ((labelfunc2 != null) ? labelfunc2(v) : null) ?? v.ToString();
                setvalue(v);
                return result;
            }, (int)min, (int)max);
            return slider;
        }

        public ABCDEFGScroll Scroll()
        {
            ABCDEFGScroll abcdefgScroll = GBF_MaybeItWillGone.Create<ABCDEFGScroll>(base.transform);
            abcdefgScroll.OnLayout();
            return abcdefgScroll;
        }

        public T Create<T>() where T : ABCDEFGLayout
        {
            T val = GBF_MaybeItWillGone.Create<T>(base.transform);
            val.OnLayout();
            return val;
        }
    }
    public class ABCDEFGLayout<T> : ABCDEFGLayout
    {
        protected ABCDEFGLayer<T>? _layer;

        public ABCDEFGLayer<T> Layer
        {
            get
            {
                return _layer;
            }
            set
            {
                _layer = value;
            }
        }

        public override void OnSwitchContent(int idTab)
        {
            Build();
        }
    }

    public class ABCDEFGScroll : ABCDEFGLayout
    {
        protected ScrollRect? _scrollRect;

        protected RectTransform? _contentTransform;

        protected VerticalLayoutGroup? _layout;

        protected ContentSizeFitter? _fitter;

        public ScrollRect ScrollRect => _scrollRect;

        public RectTransform ContentTransform => _contentTransform;

        public VerticalLayoutGroup Layout => _layout;

        public ContentSizeFitter Fitter => _fitter;

        public override void OnLayout()
        {
            _scrollRect = GBF_MaybeItWillGone.GetResource<ScrollRect>("Scrollview parchment with Header");
            RectTransform rectTransform = _scrollRect.Rect();
            rectTransform.SetParent(base.transform);
            _scrollRect.gameObject.GetComponent<LayoutElement>().flexibleHeight = 10f;
            ((RectTransform)rectTransform.Find("Header Top Parchment")).DestroyObject();
            RectTransform rectTransform2 = (RectTransform)((RectTransform)rectTransform.Find("Viewport")).Find("Content");
            rectTransform2.DestroyAllChildren();
            _contentTransform = rectTransform2;
            VerticalLayoutGroup component = rectTransform2.gameObject.GetComponent<VerticalLayoutGroup>();
            component.childControlHeight = false;
            component.childForceExpandHeight = false;
            component.childControlWidth = true;
            component.childForceExpandWidth = true;
            _layout = component;
            ContentSizeFitter contentSizeFitter = rectTransform2.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            _fitter = contentSizeFitter;
        }
    }
    public class LayerThingEditor : ABCDEFGLayer<Thing>
    {
        public override void OnLayout()
        {
            CreateTab<ThingEnchantTab>("エンチャント"._("Enchant", "附魔"), "GBF.thing.enchant");
        }
    }

    [HarmonyPatch]
    public class PatchContextMenu
    {
        // 记录上一次“装饰”过按钮的菜单实例
        private static UIContextMenu lastMenu;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InvOwner), "ShowContextMenu")]
        public static void InvOwner_ShowContextMenu(InvOwner __instance, ButtonGrid button)
        {
            // 1) 拿到当前Thing
            var thing = button?.card?.Thing;
            if (thing?.elements == null)
                return;

            // 2) 必须元素满级才允许加“升级”按钮
            const int elementId = 170022;
            var element = thing.elements.GetElement(elementId);
            if (element == null || element.vExp < element.ExpToNext)
                return;

            // 3) 拿到或新建菜单
            var menu = EClass.ui.contextMenu?.currentMenu
                       ?? EClass.ui.CreateContextMenuInteraction();
            if (menu == null)
                return;

            // 4) 如果已经对这个菜单实例加过按钮，就直接返回
            if (menu == lastMenu)
                return;
            lastMenu = menu;

            // 5) 真正添加按钮
            menu.AddButton("アップグレード"._("Upgrade", "升级"), () =>
            {
                GBF_MaybeItWillGone.CreateLayer<LayerThingEditor, Thing>(thing);
                menu.Hide();
            });

            // 6) 提示文案（可选）
            Msg.SayRaw(thing.GetName(NameStyle.Full) + "GBFlimitbeark_msg1".lang());
            Msg.SayRaw("GBFlimitbeark_msg2".lang());

            // 7) 展示
            menu.Show();
        }
    }
    public class ThingEnchantTab : ABCDEFGLayout<Thing>
    {
        private Thing thing;
        private bool hasLaidOut = false;
        private bool buttonClicked = false;
        

        public override void OnLayout()
        {
            base.OnLayout();
            if (hasLaidOut) return;
            hasLaidOut = true;

            if (this.Layer?.Data == null) return;
            thing = this.Layer.Data;
            if (thing?.elements == null) return;

            Header(thing.GetName(NameStyle.Full));

            var blacklist = new List<string> {
            "ball_Void","breathe_Void","bolt_Void","hand_Void","arrow_Void","funnel_Void",
            "miasma_Void","weapon_Void","puddle_Void","sword_Void","eleVoid","living",
            "r_life","r_mana","r_DV","r_PV","searchRange","expMod","weightMod","slowDecay",
            "corruption","resDecay","resDamage","resCurse","piety","critical","SpTeleportShort",
            "SpReturn","SpEvac","SpIdentify","SpIdentifyG","SpUncurse","SpUncurseG",
            "SpEnchantWeapon","SpEnchantWeaponGreat","SpEnchantArmor","SpEnchantArmorGreat",
            "SpMagicMap","SpLighten","SpFaith","SpChangeMaterialLesser","SpChangeMaterial",
            "SpChangeMaterialG","SpReconstruction","SpLevitate","SpMutation","SpWish",
            "SpRevive","SpRestoreBody","SpRestoreMind","SpRemoveHex","SpVanishHex",
            "SpTransmuteBroom","SpTransmutePutit","SpExterminate","SpShutterHex",
            "SpWardMonster","SpDrawMonster","SpDrawMetal","SpDrawBacker","noDamage",
            "onlyPet","permaCurse","meleeDistance","throwReturn","PDR","EDR"
        };

            if (!GBF_and_PCR_Equipment.allowAbsorbs.Value)
                blacklist.AddRange(new[] { "absorbHP", "absorbMP", "absorbSP" });
            if (!GBF_and_PCR_Equipment.allowVital.Value)
                blacklist.AddRange(new[] { "life", "mana", "vigor" });
            if (!GBF_and_PCR_Equipment.allowDefence.Value)
                blacklist.AddRange(new[] { "DV", "PV", "FPV" });
            if (!GBF_and_PCR_Equipment.allowOffence.Value)
                blacklist.AddRange(new[] { "critical", "vopal", "penetration", "evasionPerfect" });

            // 2) 拿到初始行集
            var allRows = EClass.sources.elements.rows.AsEnumerable();


            //
            allRows = allRows
    .Where(e => !(e.tag != null && e.tag.Contains("GBFskill")));

            // 3) 根据 allowInvokes 决定要不要屏蔽 spell/ActBreathe
            if (!GBF_and_PCR_Equipment.allowInvokes.Value)
            {
                allRows = allRows.Where(e =>
                    e.category != "ability"
                    || (e.group != "SPELL" && e.type != "ActBreathe")
                );
            }

            // 4) 再做其他条件的过滤
            var source = allRows
                .Where(e =>
                    (e.category == "attribute" ||
                     e.category == "skill" ||
                     e.category == "enchant" ||
                     e.category == "resist" ||
                     (e.category == "ability" && (e.group == "SPELL" || e.type == "ActBreathe")))
                    && !(e.aliasRef?.Contains("mold") ?? false)
                    && !blacklist.Contains(e.alias ?? "")
                )
                .ToArray();

            int maxRoll = Math.Max(GBF_and_PCR_Equipment.maxEnchRoll.Value, 5);
            var rng = new System.Random();
            var picks = source.OrderBy(_ => rng.Next()).Take(maxRoll).ToArray();

            const int elementId = 170022;
            var ele = thing.elements.GetElement(elementId);
            if (ele != null)
            {
                

                // 日文原文用字符串本身，后面 ._(EN, CN)
                string jp = $"<color=red>レベル：{ele.vBase}   経験：{ele.vExp}/{ele.ExpToNext}</color>";
                string en = $"<color=red>Break Level: {ele.vBase}   Exp: {ele.vExp}/{ele.ExpToNext}</color>";
                string cn = $"<color=red>突破等级：{ele.vBase}   经验：{ele.vExp}/{ele.ExpToNext}</color>";
                Header(jp._(en, cn));
            }
            

            foreach (var row in picks)
            {
                string alias = row.alias ?? "";
                string aliasRef = row.aliasRef ?? "";

                string nameJP = row.name_JP;
                string nameEN = row.name;
                string nameCN = row.GetName();

                var baseElem = thing.elements.GetElement("GBFlimitbeark");
                int vBase = baseElem?.vBase ?? 0;
                float cap = 3 + Mathf.Min(vBase / 10, 15) + Mathf.Sqrt(vBase * thing.encLV / 100f);
                int v = EClass.rnd((int)cap) + 1;

                if (row.type.Contains("Resistance") && row.group.Contains("SKILL") && row.category.Contains("resist"))
                {
                    nameJP += $"{v}獲得する";
                    nameEN = $"Add {nameEN} by {v}";
                    nameCN = $"获得{nameCN} {v}";
                }
                else if (row.type.Contains("Skill") && row.group.Contains("SKILL") && row.category.Contains("enchant") && row.categorySub.Contains("eleAttack"))
                {
                    nameJP += $"属性追加ダメージ{v}を獲得する";
                    nameEN = $"Add {nameEN} Damage by {v}";
                    nameCN = $"获得{nameCN}属性 {v}";
                }
                else if (row.type.Contains("AttbMain") && row.group.Contains("SKILL") && row.category.Contains("attribute"))
                {
                    nameJP += $"{v}上昇を獲得する";
                    nameEN = $"Increase {nameEN} by {v}";
                    nameCN = $"获得{nameCN} {v}";
                }
                else if (row.type.Contains("Skill") && row.group.Contains("SKILL") && row.category.Contains("skill"))
                {
                    nameJP += $"スキル上昇{v}を獲得する";
                    nameEN = $"Add {nameEN} Skill Bonus by {v}";
                    nameCN = $"获得{nameCN} {v}";
                }
                else if (row.type.Contains("Skill") && row.group.Contains("ENC") && row.category.Contains("enchant"))
                {
                    nameJP += $"を獲得する";
                    nameEN = $"Add {nameEN} by {v}";
                    nameCN = $"获得{nameCN}附魔 {v}";
                }

                if (alias.Contains("_") && !aliasRef.Contains("mold"))
                {
                    foreach (var row2 in source)
                    {
                        string a2 = row2.alias ?? "";
                        if (aliasRef.Trim() == a2.Trim())
                        {
                            nameJP = $"{row2.name_JP.Trim()}の{nameJP} 能力発動{v}を獲得する";
                            nameEN = $"Add {row2.name.Trim()} {nameEN} Spell Trigger by {v}";
                            nameCN = $"获得{row2.name.Trim()}{nameCN}{v}";
                            break;
                        }
                    }
                }

                Button(nameJP._(nameEN, nameCN), () =>
                {
                    var ele = thing.elements.GetElement(elementId);
                    if (ele == null || ele.vExp < ele.ExpToNext) return;

                    int leftover = ele.vExp - ele.ExpToNext;
                    thing.elements.ModBase(elementId, 1);
                    ele.vExp = Mathf.Clamp(leftover / 2, 0, ele.ExpToNext / 2);

                    if (thing.elements.GetOrCreateElement(row.id).ValueWithoutLink == 0)
                        thing.elements.ModBase(row.id, 1);
                    thing.elements.ModBase(row.id, v);

                    Msg.SayRaw(Lang.isJP ? nameJP : Lang.isEN ? nameEN : nameCN);
                    Msg.SayRaw(thing.GetName(NameStyle.Full) + "GBFlimitbeark_msg3".lang());
                    buttonClicked = true;
                    _layer.Close();
                });
            }
        }

        private void OnDestroy()
        {
            if (thing == null) return;
            if (!buttonClicked)
                Msg.SayRaw(thing.GetName(NameStyle.Full) + "GBFlimitbeark_msg4".lang());
        }
    }

}
