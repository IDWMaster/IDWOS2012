//BEGIN PLATFORM-SPECIFIC CODE

function InitEventLoop() {
    while (true) {
        KernelSpinWait();
    }
}

//END PLATFORM-SPECIFIC CODE
