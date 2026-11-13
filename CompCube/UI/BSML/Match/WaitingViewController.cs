using System.Collections;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using Reactive;
using UnityEngine;
using Zenject;

namespace CompCube.UI.BSML.Match
{
    public class WaitingViewController : ViewController
    {
        [Inject] private readonly SharedCoroutineStarter _sharedCoroutineStarter = null!;
        
        private WaitingView _view = null!;

        void Awake()
        {
            _view = new WaitingView();

            _view.Use(transform);
        }
        
        public void SetText(string text)
        {
            _sharedCoroutineStarter.Run(WaitAndSetText());
            return;

            IEnumerator WaitAndSetText()
            {
                yield return new WaitUntil(() => isActivated);
                
                _view.Text = text;
            }
        }
    }
}