using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow
{

    class RuntimeContext
    {
        private TaskCompletionSource<object> _tsc = new TaskCompletionSource<object>();

        RuntimeFunctionCall _funcCall;
        WorkflowState _state = null;
        bool _hasFinished = false;
        bool _isRunning = true;

        public RuntimeContext(RuntimeFunctionCall funcCall)
        {
            this._funcCall = funcCall;
        }

        public void SetState(WorkflowState state)
        {
            this._state = state;
        }

        public WorkflowState GetState()
        {
            return this._state;
        }

        public void SetResult(object result)
        {
            _tsc.SetResult(result);
            _hasFinished = true;
            _isRunning = false;
        }

        public Task GetTask()
        {
            return this._tsc.Task;
        }

        public void SetPauseState()
        {
            this._isRunning = false;
        }

        public void Continue()
        {
            this._isRunning = true;
        }

        public bool HasFinished() => _hasFinished;
        public bool IsRunning() => _isRunning;

        public class WorkflowState
        {
            string _name;
            WorkspaceScopeList _state;

            public WorkflowState(string name, WorkspaceScopeList state)
            {
                _name = name;
                _state = state;
            }
        }
    }

}
