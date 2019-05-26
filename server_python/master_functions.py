#!/usr/bin/python3

import socket
import os
import textwrap
import readline
import threading

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
				print("["+((25-porcent)*" ")+(porcent*"«")+"]", end= '\r' if porcent<25 else '\n' )
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
				print("["+(porcent*"»")+((25-porcent)*" ")+"]", end= '\r' if porcent<25 else '\n' )
				status = round((file_size-buff_size)/file_size,2)
		file_read.close()
	else:
		connection.send(str.encode("0"))
	return connection.recv(2048)