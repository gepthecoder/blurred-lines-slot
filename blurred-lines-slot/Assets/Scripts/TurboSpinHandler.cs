using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurboSpinHandler : MonoBehaviour
{
    [SerializeField] private Button turbo_mode_btn_;
    [SerializeField] private Image turbo_mode_symbol_img_;

    private Image turbo_mode_btn_img_;

    private Slots slot_machine_;

    private bool is_turbo_spin_ = false;

    private Sprite turbo_btn_on_sprite_;
    private Sprite turbo_btn_off_sprite_;

    private void Awake()
    {
        slot_machine_ = GetComponent<Slots>();
        turbo_mode_btn_img_ = turbo_mode_btn_.GetComponent<Image>();

        turbo_mode_btn_.onClick.AddListener(OnTurboModeBtnClicked);
    }

    private void Start()
    {
        turbo_btn_on_sprite_ = UiManager.instance_.GetStopTurboModeBtnSprite();
        turbo_btn_off_sprite_ = UiManager.instance_.GetTurboModeBtnSprite();
    }

    private void OnTurboModeBtnClicked()
    {
        is_turbo_spin_ = !is_turbo_spin_;
        Sprite btn_on_off_sprite = is_turbo_spin_ ? turbo_btn_on_sprite_ : turbo_btn_off_sprite_;
        turbo_mode_btn_img_.sprite = btn_on_off_sprite;

        turbo_mode_symbol_img_.color = is_turbo_spin_ ? Color.yellow : Color.white;

        slot_machine_.SetTurboSpinStatus(is_turbo_spin_);
    }
}
