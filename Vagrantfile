Vagrant.configure("2") do |config|
  config.vm.box = "bento/ubuntu-24.04"
  config.vm.hostname = "mywebapp"

  config.vm.network "forwarded_port", guest: 80, host: 8080

  config.vm.boot_timeout = 600

  config.vm.provider "virtualbox" do |vb|
    vb.name = "mywebapp"
    vb.cpus = 2
    vb.memory = 2048
  end

  config.vm.provision "shell", path: "scripts/install.sh", privileged: true
end