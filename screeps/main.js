// screeps/main.js

// Import the role modules
const roleHarvester = require('role.harvester');
const roleUpgrader = require('role.upgrader');
// Add other roles here as you create them, e.g.:
// const roleBuilder = require('role.builder');

module.exports.loop = function () {

    // Clear memory of dead creeps
    for(const name in Memory.creeps) {
        if(!Game.creeps[name]) {
            delete Memory.creeps[name];
            console.log('Clearing non-existing creep memory:', name);
        }
    }

    // Run logic for each creep in the game
    for(const name in Game.creeps) {
        const creep = Game.creeps[name];

        // Assign role logic based on creep memory
        if(creep.memory.role == 'harvester') {
            roleHarvester.run(creep);
        }
        if(creep.memory.role == 'upgrader') {
            roleUpgrader.run(creep);
        }
        // Add other roles here, e.g.:
        // if(creep.memory.role == 'builder') {
        //     roleBuilder.run(creep);
        // }
    }

    // Optional: Add logic for spawning new creeps if needed
    // Example: Check if you have less than 2 harvesters and try to spawn one
    /*
    const harvesters = _.filter(Game.creeps, (creep) => creep.memory.role == 'harvester');
    console.log('Harvesters: ' + harvesters.length);

    if(harvesters.length < 2) {
        const newName = 'Harvester' + Game.time;
        console.log('Spawning new harvester: ' + newName);
        // Replace 'Spawn1' with your actual Spawn name
        Game.spawns['Spawn1'].spawnCreep([WORK,CARRY,MOVE], newName,
            {memory: {role: 'harvester'}});
    }

    if(Game.spawns['Spawn1'].spawning) {
        const spawningCreep = Game.creeps[Game.spawns['Spawn1'].spawning.name];
        Game.spawns['Spawn1'].room.visual.text(
            '🛠️' + spawningCreep.memory.role,
            Game.spawns['Spawn1'].pos.x + 1,
            Game.spawns['Spawn1'].pos.y,
            {align: 'left', opacity: 0.8});
    }
    */
}
