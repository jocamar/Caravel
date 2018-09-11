guntler = caravel.Logic:GetEntity("guntler");
timeout = new:lua_Timer(10000, "print('cenas');caravel.Logic:CreateEntity('entity_types/camera.cve', 'script_entity', 'Default', true, guntler.ID);");
caravel.ProcessManager:AttachProcess(timeout);
