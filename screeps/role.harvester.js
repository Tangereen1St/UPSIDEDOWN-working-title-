// screeps/role.harvester.js
const roleHarvester = {

    /** @param {Creep} creep **/
    run: function(creep) {
        // State machine: Check if the creep is currently harvesting and is full
        if(creep.memory.harvesting && creep.store.getFreeCapacity() == 0) {
            creep.memory.harvesting = false; // Switch to transferring state
            creep.say('⚡ transfer');
	    }
	    // Check if the creep is not harvesting and is empty
	    if(!creep.memory.harvesting && creep.store.getUsedCapacity() == 0) {
	        creep.memory.harvesting = true; // Switch to harvesting state
	        creep.say('🔄 harvest');
	    }

	    // If in harvesting state
	    if(creep.memory.harvesting) {
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
        // If in transferring state
        else {
            // Find the closest spawn, extension, or tower that needs energy
            const target = creep.pos.findClosestByPath(FIND_STRUCTURES, {
                    filter: (structure) => {
                        return (structure.structureType == STRUCTURE_EXTENSION ||
                                structure.structureType == STRUCTURE_SPAWN ||
                                structure.structureType == STRUCTURE_TOWER) && // Added Tower
                                structure.store.getFreeCapacity(RESOURCE_ENERGY) > 0;
                    }
            });

            if(target) {
                // Try to transfer energy to the target
                if(creep.transfer(target, RESOURCE_ENERGY) == ERR_NOT_IN_RANGE) {
                    // Move towards the target if not in range
                    creep.moveTo(target, { visualizePathStyle: { stroke: '#ffffff' } });
                }
            } else {
                 // Optional: If no valid transfer target, maybe upgrade controller?
                 // Or move to a waiting area?
                 // console.log(creep.name + " found no valid transfer target."); // Uncomment for debugging
                 // Fallback to upgrading if no transfer targets exist
                 if(creep.upgradeController(creep.room.controller) == ERR_NOT_IN_RANGE) {
                    creep.moveTo(creep.room.controller, { visualizePathStyle: { stroke: '#ffffff' } });
                }
            }
        }
	}
};

module.exports = roleHarvester;
