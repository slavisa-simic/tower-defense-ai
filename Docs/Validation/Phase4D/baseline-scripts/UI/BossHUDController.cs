using UnityEngine;
using UnityEngine.UIElements;
using ShorelineDefense.Waves;
namespace ShorelineDefense.UI {
 [RequireComponent(typeof(UIDocument))]
 public sealed class BossHUDController : MonoBehaviour {
  [SerializeField] private BossEncounterManager encounters;
  private VisualElement panel,track,fill;private Label name,warning;
  private void OnEnable(){
   if(encounters==null)return;
   var root=GetComponent<UIDocument>().rootVisualElement;
   panel=new VisualElement{name="boss-panel",pickingMode=PickingMode.Ignore};panel.style.position=Position.Absolute;panel.style.top=105;panel.style.left=Length.Percent(28);panel.style.right=Length.Percent(28);panel.style.backgroundColor=new Color(.12f,.15f,.13f,.95f);
   name=new Label { pickingMode=PickingMode.Ignore };warning=new Label { pickingMode=PickingMode.Ignore };name.style.color=new Color(.88f,.86f,.76f);name.style.fontSize=14;name.style.unityTextAlign=TextAnchor.MiddleCenter;warning.style.unityTextAlign=TextAnchor.MiddleCenter;warning.style.color=new Color(1,.55f,.25f);
   track=new VisualElement { pickingMode=PickingMode.Ignore };track.style.height=8;track.style.backgroundColor=Color.black;fill=new VisualElement { pickingMode=PickingMode.Ignore };fill.style.height=8;fill.style.backgroundColor=new Color(.7f,.2f,.15f);track.Add(fill);panel.Add(warning);panel.Add(name);panel.Add(track);root.Add(panel);
   encounters.Changed+=Refresh;Refresh();
  }
  private void OnDisable(){if(encounters!=null)encounters.Changed-=Refresh;panel?.RemoveFromHierarchy();}
  private void Update(){if(panel!=null && warning.style.display!=DisplayStyle.None && Time.time>=encounters.MessageUntil)Refresh();}
  private void Refresh(){if(panel==null)return;var boss=encounters.ActiveBoss;bool alive=boss!=null && boss.IsTargetable;bool message=Time.time<encounters.MessageUntil;
   panel.style.display=alive||message?DisplayStyle.Flex:DisplayStyle.None;warning.style.display=message?DisplayStyle.Flex:DisplayStyle.None;warning.text=encounters.Message;
   name.style.display=track.style.display=alive?DisplayStyle.Flex:DisplayStyle.None;
   if(alive){name.text=$"{boss.Definition.DisplayName}  {boss.Health.CurrentHealth:N0} / {boss.Health.MaxHealth:N0}";fill.style.width=Length.Percent(100f*boss.Health.CurrentHealth/boss.Health.MaxHealth);}
  }
 }
}


