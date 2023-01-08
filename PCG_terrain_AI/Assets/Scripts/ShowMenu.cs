using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMenu : MonoBehaviour
{
    public GameObject menu;
    public Text btnText;

    //Either shows or hides the generation menu
    public void ChangeMenuStatus() {
        if(menu.activeInHierarchy) {
            btnText.text = "Show Menu";
            menu.SetActive(false);
        } else {
            menu.SetActive(true);
            btnText.text = "Hide Menu";
        }
    }
}
