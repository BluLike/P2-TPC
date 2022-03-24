using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTPSController : MonoBehaviour
{
    public Camera cam;
    private InputData input;
    private CharacterAnimBasedMovement characterMovement;



    // Start is called before the first frame update
    void Start()
    {
        characterMovement = GetComponent<CharacterAnimBasedMovement>();

    }

    // Update is called once per frame
    void Update()
    {
        //Get Input
        input.getInput();

        //Apply Input to Character
        characterMovement.moveCharacter(input.hMovement, input.vMovement, cam, input.jump, input.dash, input.walk);

    }
}
