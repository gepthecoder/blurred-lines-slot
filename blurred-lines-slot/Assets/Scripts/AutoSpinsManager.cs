using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AutoSpinsManager : MonoBehaviour
{
    [SerializeField] private Slots slot_manager_;

    [SerializeField] private GameObject auto_spins_ui_;

    [SerializeField] private Button auto_play_btn_;

    [SerializeField] private Button increase_auto_spins_btn_;
    [SerializeField] private Button decrease_auto_spins_btn_;
    [SerializeField] private Text auto_spin_num_txt_;

    [SerializeField] private Button start_auto_spins_btn_;


    [SerializeField] private Text auto_spin_counter_txt_;

    private bool auto_spins_shown_ = false;

    private int min_auto_spins_ = 10;
    private int max_auto_spins_ = 100;
    private int current_set_spins_;

    private bool auto_spins_started_ = false;

    private void Awake()
    { 
        //TODO: refac: uimanager->autospins
        auto_play_btn_.onClick.AddListener(OpenCloseAutoSpinsUi);

        increase_auto_spins_btn_.onClick.AddListener(IncreaseNumOfSpins);
        decrease_auto_spins_btn_.onClick.AddListener(DecreaseNumOfSpins);
        start_auto_spins_btn_.onClick.AddListener(StartAutoSpins);

        current_set_spins_ = min_auto_spins_;
        auto_spin_counter_txt_.text = "";
    }

    private void Update()
    {
        if (auto_spins_started_)
        {
            int auto_spins_left = slot_manager_.GetNumOfAutoSpinsLeft(); //TODO: expessive.. emmit event!
            if(auto_spins_left <= 0) { 
                auto_spins_started_ = false;
                auto_spin_counter_txt_.text = "";

            }
            else { auto_spin_counter_txt_.text = $"{auto_spins_left}/{current_set_spins_}"; }
        }
    }

    private void OpenCloseAutoSpinsUi()
    {
        auto_spins_shown_ = !auto_spins_shown_;
        auto_spins_ui_.SetActive(auto_spins_shown_);
    }

    private void IncreaseNumOfSpins()
    {
        current_set_spins_ += 10;
        if(current_set_spins_ > max_auto_spins_) { current_set_spins_ = 100; }
        auto_spin_num_txt_.text = current_set_spins_.ToString();
    }

    private void DecreaseNumOfSpins()
    {
        current_set_spins_ -= 10;
        if (current_set_spins_ < min_auto_spins_) { current_set_spins_ = 10; }
        auto_spin_num_txt_.text = current_set_spins_.ToString();
    }

    private void StartAutoSpins()
    {
        OpenCloseAutoSpinsUi();

        //check balance

        bool success;
        slot_manager_.TryStartAutoSpins(current_set_spins_, out success);

        if (success)
        {
            Debug.Log("Start auto spins: Success!!");
            auto_spins_started_ = true;
        }
        else
        {
            Debug.Log("Spin is in progress: Failed!!");
        }
    }



}
