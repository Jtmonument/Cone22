import subprocess
from fpdf import FPDF

job_number = ""
piece_name = "Piece Name"
outer_radius = ""
inner_radius = ""
height = ""
plate_thickness = ""
plate_location = ""
outside_radius = ""
inside_radius = ""
difference = ""
first_cutoff_radius = ""
second_cutoff_radius = ""

# plate size info
number_of_segments = ""
total_width = ""
total_length = ""
# only if number_of_segments greater than two
segment_width = ""
segment_length = ""

pdf = FPDF()
pdf.add_page()
pdf.image("processbarron_logo_dark.png", 5, 280, 85, 15)
pdf.set_font("Helvetica", size=25)
pdf.cell(200, 10, txt=piece_name, ln=1, align="C")
pdf.output("python.pdf")
subprocess.call(['python.pdf'], shell=True)
