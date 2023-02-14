using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    private float verticalRotation;
    private Vector2 mouseInput;

    public bool isLookInvert;

    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float currentSpeed;
    private Vector3 MovementDirection, movement;

    public CharacterController Player;
    private Camera Cam;
    public float jumpHeight = 18f;
    public float gravityMultiplier = 2f;

    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask layerMask;
    public float groundCheckRadius;

    public GameObject bulletImpact;
    //public float timeBetweenShots = 0.1f;
    private float shotCounter;

    public float maxHeatValue = 10f, /*heatPerShot = 0.5f,*/ coolRate = 4f, overheatCoolRate = 3f;
    private float heatCounter;
    private bool hasOverHeated;

    public Gun[] allGuns;
    private int selectedGun;
    public float muzzleDisplayTime;
    private float muzzleCounter;
    // Start is called before the first frame update
    void Start()
    {
        switchGun();
        //Locks the cursor into the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cam = Camera.main;

        UIController.instance.overheatBar.maxValue = maxHeatValue;

        Transform NewPosition = SpawnPlayers.instance.GetSpawnPoint();
        transform.position = NewPosition.position;
        transform.rotation = NewPosition.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckRadius, layerMask);
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        verticalRotation += mouseInput.y;

        //Clamps how much the player can look.
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);

        //Checks if the look is inverted
        if(isLookInvert)
        {
            //Applies an inverted rotation
            viewPoint.rotation = Quaternion.Euler(verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
        else
        {
            //Applies the view point rotation
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }

        MovementDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (Input.GetButton("Fire3"))
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        float Yvelocity = movement.y;

        movement = (transform.forward * MovementDirection.z + transform.right * MovementDirection.x).normalized * currentSpeed;

        if (!Player.isGrounded)
        {
            movement.y = Yvelocity;
        }
        else
        {
            movement.y = 0f;
        }

        if (Input.GetButton("Jump")&&isGrounded)
        {
            movement.y = jumpHeight;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMultiplier;

        Player.Move(movement * Time.deltaTime);

        if(allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
        }
        if(muzzleCounter<= 0)
        {
            allGuns[selectedGun].muzzleFlash.SetActive(false);
            muzzleCounter = 0;
        }   

        if (!hasOverHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Fire();
            }
            if (Input.GetMouseButton(0)&&allGuns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;

                if (shotCounter <= 0)
                {
                    Fire();
                }

            }

            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overheatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                hasOverHeated = false;
                UIController.instance.overheatedMessage.gameObject.SetActive(false);
            }
        }
        UIController.instance.overheatBar.value = heatCounter;
        if (heatCounter < 0)
        {
            heatCounter = 0f;
        }



        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun++;
            if (selectedGun >= allGuns.Length)
            {
                selectedGun = 0;
            }
            switchGun();
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;
            if(selectedGun < 0)
            {
                selectedGun = allGuns.Length - 1;
            }
            switchGun();
        }
        for(int i = 0; i < allGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                switchGun();
            }
        }





        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButton(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }


    }

    private void Fire()
    {
        Ray ray = Cam.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        ray.origin = Cam.transform.position;
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
          GameObject hitEffect = Instantiate(bulletImpact, hit.point + (hit.normal * 0.001f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Destroy(hitEffect, 3);
        }
        shotCounter = allGuns[selectedGun].timeBetweenShots;
        heatCounter += allGuns[selectedGun].heatPerShot;
        if(heatCounter >= maxHeatValue)
        {
            heatCounter = maxHeatValue;

            hasOverHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    private void LateUpdate()
    {
        Cam.transform.position = viewPoint.position;
        Cam.transform.rotation = viewPoint.rotation;
    }


    void switchGun()
    {
        foreach(Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }
}
