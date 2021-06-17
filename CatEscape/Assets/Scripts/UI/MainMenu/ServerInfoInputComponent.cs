using System;
using System.Collections;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using CatEscape.Network;
using CatEscape.Util;
using UnityEngine;
using UnityEngine.UI;

using Debug = CatEscape.Util.Debug;

namespace CatEscape.UI.MainMenu
{
    public class ServerInfoInputComponent : MonoBehaviour
    {
        [SerializeField] private Text _infoText;
        [SerializeField] private InputField _addressInputField;
        [SerializeField] private InputField _portInputField;
        [SerializeField] private InputField _idInputField;
        [SerializeField] private Button _startButton;

        private const string AddressRegex =
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        private bool _canInteractable1;
        private bool _canInteractable2;
        private bool _canInteractable3;

        private void Awake()
        {
            _startButton.interactable = false;
            _infoText.text = "정보를 입력해주세요.";

#if UNITY_EDITOR
            _addressInputField.text = "127.0.0.1";
            _portInputField.text = "10200";
            _idInputField.text = "test0001";
#endif
        }

        public void OnAddressInputFieldValueChanged(string text)
        {
            _canInteractable1 = Regex.IsMatch(text, AddressRegex);
            _startButton.interactable = _canInteractable1 && _canInteractable2 && _canInteractable3;
        }

        public void OnPortInputFieldValueChanged(string text)
        {
            _canInteractable2 = !string.IsNullOrEmpty(text);
            _startButton.interactable = _canInteractable1 && _canInteractable2 && _canInteractable3;
        }

        public void OnIdInputFieldValueChanged(string text)
        {
            _canInteractable3 = !string.IsNullOrEmpty(text);
            _startButton.interactable = _canInteractable1 && _canInteractable2 && _canInteractable3;
        }

        public void OnButtonPressed()
        {
            StartCoroutine(ConnectToServer());
        }

        private IEnumerator ConnectToServer()
        {
            SetInteractable(false);

            var address = _addressInputField.text;
            var port = int.Parse(_portInputField.text);
            var id = _idInputField.text;
            _infoText.text = "연결 중...";

            var task = NetworkManager.ConnectToServerAsync(address, port, id);

            yield return task.AsCoroutine();

            if (task.Exception != null)
            {
                foreach (var ie in task.Exception.InnerExceptions)
                {
                    var e = ie.InnerException ?? ie;
                    switch (e)
                    {
                        case SocketException se:
                            _infoText.text = "서버에 연결할 수 없습니다.";
                            Debug.LogError($"socket exception error code: {se.ErrorCode}");
                            break;

                        case TimeoutException _:
                            _infoText.text = "서버로부터 응답을 받지 못했습니다.";
                            break;

                        case ConnectionFailException fail:
                            _infoText.text = fail.Reason switch
                            {
                                ReplyPacket.Reasons.NameOfPlayerIsAlreadyConnected => "서버에 같은 아이디를 사용하는 플레이어가 있습니다.",
                                ReplyPacket.Reasons.ServerIsFull => "서버가 꽉 찼습니다.",
                                _ => "알 수 없는 이유로 서버에 연결하지 못했습니다."
                            };
                            break;

                        default:
                            _infoText.text = e.Message ?? "알 수없는 예외 발생";
                            break;
                    }

                    Debug.LogException(e);
                }

                SetInteractable(true);
                yield break;
            }

            _infoText.text = "다른 유저를 기다리고 있습니다.";
            SceneLoader.LoadScene("Game");
        }

        private void SetInteractable(bool interactable)
        {
            _addressInputField.interactable = interactable;
            _portInputField.interactable = interactable;
            _idInputField.interactable = interactable;
            _startButton.interactable = interactable;
        }
    }
}
