//BEGIN PLATFORM-SPECIFIC CODE
idwos_kernel_alive = true;
function Shutdown() {
    idwos_kernel_alive = false;
}
function Kill() {
    idwos_kernel_alive = false;
    IDWOS.Threading.ThreadContext.SendMsg({ opcode: 3 });
    
    }
function InitEventLoop() {
    while (idwos_kernel_alive) {
        KernelSpinWait();
    }
}

//END PLATFORM-SPECIFIC CODE
