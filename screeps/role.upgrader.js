// screeps/role.upgrader.js
const roleUpgrader = {

    /** @param {Creep} creep **/
    run: function(creep) {

        // State machine: Check if the creep is currently upgrading and has no energy left
        if(creep.memory.upgrading && creep.store[RESOURCE_ENERGY] == 0) {
            creep.memory.upgrading = false; // Switch to harvesting state
            creep.say('🔄 harvest');
        }
        // Check if the creep is not upgrading and is full of energy
        if(!creep.memory.upgrading && creep.store.getFreeCapacity() == 0) {
            creep.memory.upgrading = true; // Switch to upgrading state
            creep.say('⚡ upgrade');
        }

        // If in upgrading state
        if(creep.memory.upgrading) {
            // Try to upgrade the controller
            if(creep.upgradeController(creep.room.controller) == ERR_NOT_IN_RANGE) {
                // Move towards the controller if not in range
                creep.moveTo(creep.room.controller, { visualizePathStyle: { stroke: '#ffffff' } });
            }
        }
        // If in harvesting state
        else {
            // Find the closest active energy source
            const source = creep.pos.findClosestByPath(FIND_SOURCES_ACTIVE);
            if(source) {
                // Try to harvest the source
                if(creep.harvest(source) == ERR_NOT_IN_RANGE) {
                    // Move towards the source if not in range
                    creep.moveTo(source, { visualizePathStyle: { stroke: '#ffaa00' } });
                }
            }
        }
    }
};

module.exports = roleUpgrader;
