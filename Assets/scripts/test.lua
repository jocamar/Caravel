guntler = caravel.Logic:GetEntity("guntler");
timeout = new:lua_Timer(10000, "log:Info('cenas');caravel.Logic:CreateEntity('entity_types/camera.cve', 'script_entity', 'Default', true, guntler.ID);");
log:Info("Player view "..tostring(caravel:GetPlayerView(caravel.PlayerOne).ID));

caravel.EventManager:AddListener("Cv_Event_PlaySound", "log:Info('tocou som');");
