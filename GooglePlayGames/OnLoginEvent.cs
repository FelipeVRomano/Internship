using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OnLoginEvent
{
    //código de exemplo que desativa e ativa gameobjects quando o player realiza o login no app. Pode ser utilizado para trocar de cena ao fazer o login também, por exemplo.
    public GameObject screenObj;
    public bool activateGameObject;
}
