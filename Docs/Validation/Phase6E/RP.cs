using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using ShorelineDefense.Persistence;
using ShorelineDefense.Progression;
public static class Phase6ERP
{
    public static string Run()
    {
        var catalog=ResearchCatalog.Load();var rows=new List<object>();
        foreach(var s in new[]{new{label="weak",wave=3,bosses=0},new{label="average",wave=15,bosses=0},new{label="boss",wave=25,bosses=1},new{label="good",wave=40,bosses=1},new{label="late",wave=60,bosses=2}})
        {
            var profile=new PlayerProfileService(new LocalJsonStore(Path.Combine(Path.GetTempPath(),"BorderDefenders6ERP",Guid.NewGuid().ToString())));
            var tree=new ResearchTreeService(profile,catalog,()=>true);var awards=new List<object>();int first=0,mid=0,cap=0,full=0;
            for(int run=1;run<=40;run++)
            {
                var record=new RunRecord{runId=Guid.NewGuid().ToString(),mapId=run%2==0?"river_border":"industrial_port",wave=s.wave,completedWaves=s.wave-1,bosses=s.bosses,dredgers=s.bosses>0?1:0,broodmothers=s.bosses>1?1:0,homicides=1000};
                if(!profile.CompleteRun(record,catalog.rewards,out var award,out var error))throw new Exception(error);
                var bought=new List<string>();bool progress;
                do{progress=false;foreach(var node in catalog.nodes.OrderBy(n=>n.tier).ThenBy(n=>n.costRP))if(tree.Status(node)==ResearchNodeStatus.Available){if(!tree.TryPurchase(node.id,out error))throw new Exception(error);bought.Add(node.id);progress=true;if(first==0)first=run;if(node.tier==3&&mid==0)mid=run;if(node.tier==4&&cap==0)cap=run;}}while(progress);
                if(profile.Current.unlockedResearchNodes.Count==20&&full==0)full=run;
                if(run<=20)awards.Add(new{run,award.Total,bought,remaining=profile.ResearchPoints});
            }
            rows.Add(new{s.label,s.wave,s.bosses,firstNodeRun=first,firstTier3Run=mid,firstCapstoneRun=cap,fullTreeRun=full,awards});
        }
        File.WriteAllText("Docs/Validation/Phase6E/rp-progression.json",JsonConvert.SerializeObject(new{method="Synthetic failed runs at 1000 Homicides through actual reward/milestone/purchase/persistence services, 610 RP tree. Zero values mean not reached in 40 runs. No Grant RP.",rows},Formatting.Indented));return "RP scenarios written";
    }
}
