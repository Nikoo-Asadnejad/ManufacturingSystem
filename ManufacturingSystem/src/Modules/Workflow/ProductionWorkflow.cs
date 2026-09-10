using ManufacturingSystem.Modules.Sensors;
using ManufacturingSystem.Modules.Stages;
using ManufacturingSystem.Modules.Workflow.Rules;

namespace ManufacturingSystem.Modules.Workflow;

internal sealed class ProductionWorkflow(IEnumerable<IRule> rules) : IWorkflow
{
    private readonly IRule[] _rules = rules.ToArray();
    public WorkflowId Id => WorkflowId.Production;
    public IReadOnlyCollection<StageId> SelectStages(SensorSnapshot snapshot)
    {
        var selectedStages = new HashSet<StageId>();

        foreach (var rule in _rules)
        {
            if (rule.Matches(snapshot))
            {
                selectedStages.UnionWith(rule.GetStages(snapshot));
            }
        }

        return [.. selectedStages.OrderBy(stage => stage)];
    }
}
