using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ResetService : ServiceBase<ResetService>
{

    private static List<IResetable> _s_resetables = new();


    private static List<Action> _resetActions = new();
    private static Action OnCompleteResetAction;

    public void Reset()
    {
        ResetSaveData();
        OnCompleteResetAction?.Invoke();
    }


    private void ResetSaveData()
    {
        foreach (var s in _s_resetables)
        {
            SaveLoadManager.Delete(s.saveData);
        }
    }

    public void Register(IResetable resetable, Action OnResetAction = null)
    {
        _s_resetables.Add(resetable);

        if (OnResetAction != null)
        {
            _resetActions.Add(OnResetAction);
            OnCompleteResetAction += OnResetAction;
        }
    }

    private void OnDisable()
    {
        foreach (var action in _resetActions)
        {
            OnCompleteResetAction -= action;
        }
    }
}