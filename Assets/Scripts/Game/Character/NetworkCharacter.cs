﻿using JoyWay.Infrastructure.Factories;
using JoyWay.Services;
using Mirror;
using UnityEngine;

namespace JoyWay.Game.Character
{
    public class NetworkCharacter : NetworkBehaviour
    {
        [SerializeField] private CharacterContainer _container;
        private CameraService _cameraService;
        private InputService _inputService;

        private bool _isOwner;
        
        public void Initialize(
            bool isOwner,
            bool isHost,
            InputService inputService,
            CameraService cameraService,
            ProjectileFactory projectileFactory)
        {
            _isOwner = isOwner;
            _inputService = inputService;
            _cameraService = cameraService;
            
            if (isHost)
            {
                _container.HealthComponent.Initialize();
            }

            if (_isOwner)
            {
                _inputService.Move += _container.MovementComponent.Move;
                _inputService.Jump += _container.MovementComponent.Jump;
                _inputService.Interact += _container.InteractionComponent.Interact;
                _inputService.Fire += _container.ShootingComponent.Fire;
                _cameraService.LookDirectionUpdated += _container.LookComponent.UpdateLookDirection;
                
                _container.LookComponent.Initialize(_cameraService);
                _container.MovementComponent.Initialize(_container.RotationComponent);
                _container.InteractionComponent.Initialize(_cameraService);
            }
            
            _container.LookComponent.LookDirectionChanged += _container.RotationComponent.ChangeLookDirection;
            _container.HealthComponent.HealthChanged += (_, __) => _container.DamageDisplayComponent.DisplayDamageTaken();
            _container.HealthComponent.HealthChanged += _container.HealthBarUI.SetHealth;
            
            _container.ShootingComponent.Initialize(_cameraService, projectileFactory);
            _container.DamageDisplayComponent.Initialize();
            _container.HealthBarUI.Initialize(_container.HealthComponent.Health, _container.HealthComponent.MaxHealth);
        }

        private void OnDestroy()
        {
            if (_isOwner)
            {
                _cameraService.LookDirectionUpdated -= _container.LookComponent.UpdateLookDirection;
                _inputService.Fire -= _container.ShootingComponent.Fire; // something wrong with unsubscribe, because after respawn character cant attack
                _inputService.Move -= _container.MovementComponent.Move;
                _inputService.Jump -= _container.MovementComponent.Jump;
                _inputService.Interact -= _container.InteractionComponent.Interact;
            }
            
            _container.LookComponent.LookDirectionChanged -= _container.RotationComponent.ChangeLookDirection;
            _container.HealthComponent.HealthChanged -= _container.HealthBarUI.SetHealth;
        }
    }
}