guntler = caravel.Logic:GetEntity("guntler");
timeout = new:lua_Timer(10000, "log:Info('cenas');caravel.Logic:CreateEntity('entity_types/camera.cve', 'script_entity', 'Default', true, guntler.ID);");
caravel.ProcessManager:AttachProcess(timeout);
log:Info("Player view "..tostring(caravel:GetPlayerView(caravel.PlayerOne).ID));
