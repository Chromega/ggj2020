formats = ['obj', 'mtl', 'png']

formats.each do |format|
  Dir.glob("Art/*.#{format}").sort.each do |file|
    match = /([a-z]+)-[0-9]+-([a-z0-9\-]+)\.(.*)/.match(file)
    if match
      rename = "#{match[1]}-#{match[2]}.#{match[3]}"
      File.rename("#{file}", "Art/#{rename}")
      puts "Renamed #{file} to #{rename}"
    end
  end
end