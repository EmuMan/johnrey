using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
   public void PlayGame() 
   {
      SceneManager.LoadScene(2);
   }

   public void QuitGame() 
   {
      Application.Quit();
   }
}
