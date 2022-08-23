import subprocess, sys
from fpdf import FPDF


def get_args():
    try:
        # Inputs
        global piece_name
        piece_name = sys.argv[1]
        global job_number
        job_number = "Job Number: " + sys.argv[2]
        global inputs
        inputs = {
            "Type": "Inner Plates" if int(sys.argv[3]) == 0 else "Outer Plates",
            "Outer Radius": sys.argv[4] + " in.",
            "Inner Radius": sys.argv[5] + " in.",
            "Height": sys.argv[6] + " in.",
            "Plate Thickness": sys.argv[7] + " in.",
        }

        # Outputs
        global outputs
        outputs = {
            "Outside Radius": sys.argv[8] + " in.",
            "Inside Radius": sys.argv[9] + " in.",
            "Difference": sys.argv[10] + " in.",
            "Degrees": sys.argv[11],
            "First Cut-Off Radius": sys.argv[12] + " in.",
            "Second Cut-Off Radius": sys.argv[13] + " in."
        }
        global plate_size
        plate_size = {
            "Number of Segments": sys.argv[14],
            "Total Width": sys.argv[15] + " in.",
            "Total Length": sys.argv[16] + " in."
        }
        if int(sys.argv[14]) > 1:
            plate_size.update({"Width of one segment": sys.argv[17] + " in.", "Length of one segment": sys.argv[18] + " in."})
    except:
        print("Insufficient arguments")



def main():
    # Setup
    get_args()
    pdf = FPDF()
    pdf.add_page()
    pdf.image("processbarron_logo_dark.png", 5, 280, 85, 15)
    pdf.set_font("Helvetica", size=15)
    pdf.cell(180, 1, txt="Cone Development", ln=1, align="R")
    pdf.cell(180, 10, txt=job_number, ln=2, align="R")
    pdf.set_font("Helvetica", size=25)
    pdf.cell(200, 10, txt=piece_name, ln=3, align="L")
    pdf.set_font("Helvetica", size=15)
    pdf.cell(200, 10, ln=4)

    # Inputs
    index = 5
    pdf.set_font("Helvetica", style="B", size=20)
    pdf.cell(200, 10, ln=index)
    index += 1
    pdf.cell(200, 10, txt="Inputs", ln=index, align="C")
    index += 1
    pdf.set_font("Helvetica", size=15)
    for item in inputs:
        pdf.cell(200, 10, txt=item + ": " + inputs.get(item), ln=index, align="L")
        index += 1
    
    # Outputs
    pdf.set_font("Helvetica", style="B", size=20)
    pdf.cell(200, 10, ln=index)
    index += 1
    pdf.cell(200, 10, txt="Outputs", ln=index, align="C")
    index += 1
    pdf.set_font("Helvetica", size=15)
    for item in outputs:
        pdf.cell(200, 10, txt=item + ": " + outputs.get(item), ln=index, align="L")
        index += 1
    
    # Plate Size Information
    pdf.set_font("Helvetica", style="B", size=20)
    pdf.cell(200, 10, ln=index)
    index += 1
    pdf.cell(200, 10, txt="Plate Size Information", ln=index, align="C")
    index += 1
    pdf.set_font("Helvetica", size=15)
    for item in plate_size:
        pdf.cell(200, 10, txt=item + ": " + plate_size.get(item), ln=index, align="L")
        index += 1
    user = str(subprocess.Popen(["echo", "%USERNAME%"], shell=True, stdout=subprocess.PIPE).communicate()[0].decode("utf-8")[:-2])
    path = "\\\\01it01vr\\User_Folders_HQ\\" + user + "\\Downloads\\" + piece_name + ".pdf"
    pdf.output(path)
    subprocess.call([path], shell=True)


main()
